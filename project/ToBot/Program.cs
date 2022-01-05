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

using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ToBot.App;
using Topshelf;

namespace ToBot
{
    public class Program
    {
        private static string StartupPath { get; set; }

        public static void Main(string[] args)
        {
            if (new [] { "-h", "h", "-help", "help" }.Any(x => args.Contains(x)))
            {
                StringBuilder sbCommands = new StringBuilder()
                    .AppendLine($"`-h | h | -help | help` - Prints help.")
                    .AppendLine($"`-scsn` - Service's service name.")
                    .AppendLine($"`-scdn` - Service's display name.")
                    .AppendLine($"`-scd` - Service's description.")
                    .AppendLine($"`-sp` - Startup path.")
                    .AppendLine($"`-ns` - Path to netstandard dll.")
                    .AppendLine($"`-c` - Path to config file.")
                    .AppendLine($"`-m` - Module to host. May be specified multiple times to host multiple modules.")
                    ;

                Console.WriteLine(sbCommands.ToString());
            }
            else
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                args = GetArguments();

                ArgumentsParser parser = new ArgumentsParser();

                string defaultName = $"ToBot_{Guid.NewGuid().ToString()}";
                string serviceName = parser.GetConfigValue(args, "-scsn", defaultName);
                string displayName = parser.GetConfigValue(args, "-scdn", defaultName);
                string description = parser.GetConfigValue(args, "-scd", string.Empty);

                StartupPath = parser.GetConfigValue(args, "-sp", string.Empty);

                TopshelfExitCode rc = HostFactory.Run(x =>
                {
                    x.Service<ServiceHost>(s =>
                    {
                        s.ConstructUsing(name => new ServiceHost(args));
                        s.WhenStarted(tc => tc.Start());
                        s.WhenStopped(tc => tc.Stop());
                    });

                    x.RunAsLocalSystem();

                    x.SetDescription(description);
                    x.SetDisplayName(displayName);
                    x.SetServiceName(serviceName);
                });

                int exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());

                Environment.ExitCode = exitCode;
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string assemblyName = args.Name.Split(',').First().Trim();
            string fileName = $"{assemblyName}.dll";
            string fullFilePath = Path.Combine(StartupPath, fileName);

            if (fullFilePath.EndsWith(".resources.dll"))
            {
                return null;
            }

            if (!File.Exists(fullFilePath))
            {
                throw new FileNotFoundException($"Dynamic assembly resolve could not find file `{fullFilePath}`.");
            }

            Assembly assembly = Assembly.LoadFile(fullFilePath);

            if (assembly == null)
            {
                throw new InvalidOperationException($"Assembly of file `{fullFilePath}` failed to load.");
            }

            return assembly;
        }

        private static string[] GetArguments()
        {
            const string configFileName = "appsettings.json";
            const string argumentsKey = "arguments";

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddJsonFile(configFileName, true, true);

            IConfigurationRoot config = builder.Build();

            return config.GetSection(argumentsKey).Get<string[]>();
        }
    }
}
