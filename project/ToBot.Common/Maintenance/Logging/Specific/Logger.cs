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
using ToBot.Common.Maintenance.Logging.Writers;

namespace ToBot.Common.Maintenance.Logging.Specific
{
    public class Logger
        : ILogger
    {
        private readonly object _syncObject = new object();

        public Logger()
        {
            LogQueue = new Queue<string>();
            Writers = new List<IWriter>();
        }

        private Queue<string> LogQueue { get; }

        private List<IWriter> Writers { get; }

        public ILogger Add(IWriter writer)
        {
            lock (_syncObject)
            {
                Writers.Add(writer);
            }

            return this;
        }

        public ILogger Remove(IWriter writer)
        {
            lock (_syncObject)
            {
                Writers.Remove(writer);
            }

            return this;
        }

        public void LogMessage(LogLevel lvl, string module, string msg)
        {
            Log($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss zzz}] [{lvl}] [{module}] {msg}");
        }

        public void LogMessage(LogLevel lvl, string module, string msg, DateTime timestamp)
        {
            Log($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss zzz}] [{lvl}] [{module}] On {timestamp:yyyy-MM-dd HH:mm:ss zzz}: {msg}");
        }

        private void Log(string str)
        {
            lock (_syncObject)
            {
                if (Writers.Count > 0)
                {
                    while (LogQueue.Count > 0)
                    {
                        Write(LogQueue.Dequeue());
                    }

                    Write(str);
                }
                else
                {
                    LogQueue.Enqueue(str);
                }
            }
        }

        private void Write(string str)
        {
            foreach (IWriter writer in Writers)
            {
                writer.Write(str);
            }
        }
    }
}
