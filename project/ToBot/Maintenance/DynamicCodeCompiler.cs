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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using ToBot.Common.Maintenance.Logging;

namespace ToBot.Maintenance
{
    public sealed class DynamicCodeCompiler
    {
        // The MIT License (MIT)
        // 
        // Copyright (c) 2016 Joel Martinez
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
        //
        // https://github.com/joelmartinez/dotnet-core-roslyn-sample
        public Assembly Compile(ILogger logger, string netStandardDllPath, string[] localDlls, string codeToCompile)
        {
            logger.LogMessage(LogLevel.Debug, nameof(DynamicCodeCompiler), $"Code to compile\r\n: {codeToCompile}");

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(codeToCompile);

            string assemblyName = Path.GetRandomFileName();
            string[] refPaths = new[]
            {
                typeof(DSharpPlus.BaseModule).Assembly.Location,
                typeof(DSharpPlus.CommandsNext.Command).Assembly.Location,
                typeof(DSharpPlus.Interactivity.MessageContext).Assembly.Location,
                typeof(Object).Assembly.Location,
                Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll"),
                netStandardDllPath
            }
            .Concat(localDlls)
            .ToArray()
            ;

            MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();

            logger.LogMessage(LogLevel.Debug, nameof(DynamicCodeCompiler), "Adding the following references");

            foreach (string path in refPaths)
            {
                logger.LogMessage(LogLevel.Debug, nameof(DynamicCodeCompiler), path);
            }

            logger.LogMessage(LogLevel.Debug, nameof(DynamicCodeCompiler), "Compiling ...");

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (MemoryStream ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    logger.LogMessage(LogLevel.Error, nameof(DynamicCodeCompiler), "Compilation failed!");

                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

                    StringBuilder sbErrors = new StringBuilder();

                    foreach (Diagnostic diagnostic in failures)
                    {
                        sbErrors.AppendLine(string.Format("\t{0}: {1}", diagnostic.Id, diagnostic.GetMessage()));
                    }

                    string errors = sbErrors.ToString();

                    logger.LogMessage(LogLevel.Error, nameof(DynamicCodeCompiler), errors);

                    throw new InvalidOperationException(errors);
                }
                else
                {
                    logger.LogMessage(LogLevel.Debug, nameof(DynamicCodeCompiler), "Compilation successful! Returning assembly");

                    ms.Seek(0, SeekOrigin.Begin);

                    Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);

                    return assembly;
                }
            }
        }
    }
}
