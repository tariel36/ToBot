// The MIT License (MIT)
//
// Copyright (c) 2022 tariel36
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using ToBot.App;
using ToBot.Common.Attributes;
using ToBot.Common.Maintenance.Logging;
using ToBot.Maintenance;
using ToBot.Plugin.Plugins;

namespace ToBot.Plugins
{
    public class PluginLoader
    {
        private const string PluginFilePrefix = "ToBot.Plugin.";
        private const string BasePluginFileName = "ToBot.Plugin.dll";
        private const string CommandProxySuffix = "CommandProxy";
        private const string PluginNamePrefix = "Plugin";

        public PluginLoader(ILogger logger)
        {
            Logger = logger;
            LoadedPlugins = new Dictionary<string, LoadedAssembly>();
            Compiler = new DynamicCodeCompiler();
        }

        private ILogger Logger { get; }

        private DynamicCodeCompiler Compiler { get; }

        private Dictionary<string, LoadedAssembly> LoadedPlugins { get; }

        public ICollection<PluginInfo> Load(string netStandardDllPath, string pluginsPath, ICollection<string> pluginsToLoad)
        {
            if (!Directory.Exists(pluginsPath))
            {
                throw new DirectoryNotFoundException($"Directory with path `{pluginsPath}` does not exist.");
            }

            string thisAssemblyPath = Path.Combine(pluginsPath, "ToBot.dll");

            List<PluginInfo> plugins = new List<PluginInfo>();

            Type baseType = typeof(BasePlugin);

            foreach (string filePath in Directory.EnumerateFiles(pluginsPath).Where(x => IsPlugin(x) && pluginsToLoad.Contains(Path.GetFileNameWithoutExtension(x).Split('.').Last())))
            {
                Logger.LogMessage(LogLevel.Debug, nameof(PluginLoader), $"Loading plugin `{filePath}` start");

                Logger.LogMessage(LogLevel.Debug, nameof(PluginLoader), $"Loading plugin assembly");

                Assembly pluginAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(filePath);

                // Get additional DLLs
                Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                string[] dlls = pluginAssembly.GetReferencedAssemblies()
                    .Select(x => loadedAssemblies.FirstOrDefault(y => string.Equals(x.FullName, y.FullName))?.Location)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Concat(new[] { filePath, thisAssemblyPath })
                    .Distinct()
                    .ToArray()
                ;

                Logger.LogMessage(LogLevel.Debug, nameof(PluginLoader), $"Finding plugin type");

                Type pluginType = pluginAssembly.GetExportedTypes().FirstOrDefault(x => baseType.IsAssignableFrom(x));

                if (pluginType == null)
                {
                    throw new InvalidOperationException($"File `{filePath}` is not a plugin!");
                }

                Logger.LogMessage(LogLevel.Debug, nameof(PluginLoader), $"Trying to create command proxy class assembly");

                Assembly proxyAssembly = CreateProxyClass(netStandardDllPath, dlls, pluginType);

                Logger.LogMessage(LogLevel.Debug, nameof(PluginLoader), $"Trying to find created proxy class type");

                Type commandsType = proxyAssembly.GetExportedTypes().FirstOrDefault(x => x.Name.EndsWith(CommandProxySuffix));

                if (commandsType == null)
                {
                    throw new InvalidOperationException($"Command proxy missing in `{filePath}`!");
                }

                Logger.LogMessage(LogLevel.Debug, nameof(PluginLoader), $"Plugin assembly and proxy assembly loaded successfully");

                LoadedPlugins.Add(filePath, new LoadedAssembly() { PluginAssembly = pluginAssembly, ProxyAssembly = proxyAssembly });

                string dllLastPart = Path.GetFileNameWithoutExtension(filePath).Split('.').Last();

                plugins.Add(new PluginInfo(dllLastPart, pluginType, commandsType));
            }

            Logger.LogMessage(LogLevel.Debug, nameof(PluginLoader), $"Loaded plugins: {string.Join(", ", plugins.Select(x => x.Name))}");

            return plugins;
        }

        private bool IsPlugin(string fullFilePath)
        {
            string fileName = Path.GetFileName(fullFilePath);
            return !string.Equals(fileName, BasePluginFileName, StringComparison.InvariantCultureIgnoreCase)
                && fileName.StartsWith(PluginFilePrefix);
        }

        private Assembly CreateProxyClass(string netStandardDllPath, string[] dlls, Type pluginType)
        {
            string pluginNamespace = pluginType.Namespace;
            string proxyName = pluginType.Name + CommandProxySuffix;
            string pluginClassName = pluginType.Name;

            // {0} - Plugin namespace
            // {1} - Proxy class name
            // {2} - Plugin name
            // {3} - Methods placeholder

            const string ClassTemplate = @"
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;
using ToBot.Communication.Commands;
using {0};

namespace ToBot.Plugins.CommandProxies
{
    public class {1}
    {
        private static readonly Type PluginType = typeof({2});

        public {1}(PluginsManager pluginsManager)
        {
            PluginsManager = pluginsManager;
        }

        private PluginsManager PluginsManager { get; }

{3}
    }
}
";

            // {0} - Plugin class name
            // {1} - Command name
            // {2} - Description in "";

            const string ParamsStringArgsTemplate = @"
#region {1}

[Command({0}.Prefix + nameof({1}))]
[Description({2})]
public async Task {1}(CommandContext ctx, params string[] args)
{
    await PluginsManager.OnMessage(PluginType, nameof({1}), ctx, CommandArguments.Create(nameof(args), args));
}

#endregion
";

            const string EmptyTemplate = @"
#region {1}

[Command({0}.Prefix + nameof({1}))]
[Description({2})]
public async Task {1}(CommandContext ctx)
{
    await PluginsManager.OnMessage(PluginType, nameof({1}), ctx, CommandArguments.Empty);
}

#endregion
";

            const string SpecificArgsTemplate = @"
#region {1}

[Command({0}.Prefix + nameof({1}))]
[Description({2})]
public async Task {1}(CommandContext ctx, {3})
{
    await PluginsManager.OnMessage(PluginType, nameof({1}), ctx, {4});
}

#endregion
";

            List<string> methods = new List<string>();

            foreach (MethodInfo methodInfo in pluginType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetCustomAttribute<IsCommandAttribute>() != null))
            {
                string methodCode;

                IsCommandAttribute.Argument[] methodArgs = methodInfo.GetCustomAttribute<IsCommandAttribute>().Arguments;

                if (methodArgs == null || methodArgs.Length == 0)
                {
                    methodCode = EmptyTemplate.Replace("{0}", pluginClassName)
                        .Replace("{1}", methodInfo.Name)
                        .Replace("{2}", "\"\"");
                }
                else if (methodArgs != null && methodArgs.Length == 1 && string.Equals(methodArgs[0].Name, "args", StringComparison.InvariantCultureIgnoreCase) && methodArgs[0].TypeName == "string[]")
                {
                    methodCode = ParamsStringArgsTemplate.Replace("{0}", pluginClassName)
                        .Replace("{1}", methodInfo.Name)
                        .Replace("{2}", "\"\"");
                }
                else
                {
                    const string ArgsBase = "CommandArguments.Create({0})";

                    string declArgs = string.Join(", ", methodArgs.Select(x => $"{(x.IsParams ? "params " : string.Empty)}{x.TypeName} {x.Name}{(string.IsNullOrWhiteSpace(x.DefaultValue) ? string.Empty : $" = {x.DefaultValue}")}"));
                    string callArgs = ArgsBase.Replace("{0}", string.Join(", ", methodArgs.SelectMany(x => new[] { $"nameof({x.Name})", x.Name })));

                    methodCode = SpecificArgsTemplate.Replace("{0}", pluginClassName)
                        .Replace("{1}", methodInfo.Name)
                        .Replace("{2}", "\"\"")
                        .Replace("{3}", declArgs)
                        .Replace("{4}", callArgs);
                }

                methods.Add(methodCode);
            }

            string classCode = ClassTemplate.Replace("{0}", pluginNamespace).Replace("{1}", proxyName).Replace("{2}", pluginClassName).Replace("{3}", string.Join("\r\n\r\n", methods));

            
            return Compiler.Compile(Logger, netStandardDllPath, dlls, classCode);
        }
        
        private class LoadedAssembly
        {
            public Assembly PluginAssembly;
            public Assembly ProxyAssembly;
        }
    }
}
