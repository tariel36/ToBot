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

using System.Collections.Generic;
using System.Linq;
using ToBot.Common.Extensions;
using ToBot.Communication.Commands;
using ToBot.Plugin.Plugins;

namespace ToBot.Plugins
{
    public class RegisteredPlugin
        : Common.Disposables.IDisposable
    {
        public RegisteredPlugin(BasePlugin plugin,
            string commandsPrefix,
            Dictionary<string, RegisteredCommand> registeredCommands)
        {
            Plugin = plugin;
            CommandsPrefix = commandsPrefix;

            RegisteredCommands = registeredCommands ?? new Dictionary<string, RegisteredCommand>();
        }

        public string Name { get { return Plugin.Name; } }

        public bool IsDisposed { get; private set; }

        public BasePlugin Plugin { get; }

        private Dictionary<string, RegisteredCommand> RegisteredCommands { get; }

        private string CommandsPrefix { get; }

        public ICollection<RegisteredCommand> GetCommands()
        {
            return RegisteredCommands.Values.ToList();
        }

        public RegisteredCommand GetCommand(string name)
        {
            string fullName = CommandsPrefix + name;

            if (!RegisteredCommands.TryGetValue(fullName, out RegisteredCommand command))
            {
                RegisteredCommands.TryGetValue(name, out command);
            }

            return command;
        }

        public void Dispose()
        {
            Plugin.SafeDispose();

            IsDisposed = true;
        }
    }
}
