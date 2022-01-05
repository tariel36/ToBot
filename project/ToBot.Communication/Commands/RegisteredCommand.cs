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
using System.Threading.Tasks;
using ToBot.Communication.Messaging;

namespace ToBot.Communication.Commands
{
    public sealed class RegisteredCommand
    {
        public RegisteredCommand(CommandDelegate invocator,
            string name,
            string methodName,
            string descr,
            ICollection<CommandParameter> parameters,
            ICollection<string> aliases,
            ICollection<string> requiredRoles,
            int requiredPermissions)
        {
            Invocator = invocator;
            Name = name;
            MethodName = methodName;
            Description = descr;
            RequiredPermissions = requiredPermissions;

            ParametersInternal = parameters == null ? new List<CommandParameter>() : new List<CommandParameter>(parameters);
            AliasesInternal = aliases == null ? new List<string>() : new List<string>(aliases);
            RequiredRolesInternal = requiredRoles == null ? new List<string>() : new List<string>(requiredRoles);
        }

        public string Name { get; }

        public string MethodName { get; }

        public string Description { get; }

        public ICollection<string> RequiredRoles { get { return RequiredRolesInternal; } }

        public int RequiredPermissions { get; }

        public CommandDelegate Invocator { get; }

        public ICollection<CommandParameter> Parameters { get { return ParametersInternal; } }

        public ICollection<string> Aliases { get { return AliasesInternal; } }

        private List<string> RequiredRolesInternal { get; }

        private List<CommandParameter> ParametersInternal { get; }

        private List<string> AliasesInternal { get; }

        public Task Execute(CommandExecutionContext ctx)
        {
            return Invocator(ctx);
        }

        public class CommandParameter
        {
            public CommandParameter(string name, string descr, bool isRequired)
            {
                Name = name;
                Description = descr;
                IsRequired = isRequired;
            }

            public string Name { get; }

            public string Description { get; }

            public bool IsRequired { get; }
        }
    }
}
