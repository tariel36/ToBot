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

namespace ToBot.Communication.Commands
{
    public class CommandArguments
    {
        public static readonly CommandArguments Empty = new CommandArguments();

        public CommandArguments()
        {
            Values = new Dictionary<string, object>();
        }

        private Dictionary<string, object> Values { get; }

        public static CommandArguments Create(params object[] values)
        {
            CommandArguments args = new CommandArguments();

            if (values != null)
            {
                for (int i = 1; i < values.Length; i += 2)
                {
                    args.Set(values[i - 1].ToString(), values[i]);
                }
            }

            return args;
        }

        public void Set(string key, object value)
        {
            Values[key] = value;
        }

        public T Get<T>(string key)
        {
            return (T) Values[key];
        }

        public T TryGet<T>(string key)
        {
            if (Values.ContainsKey(key) && Values[key] is T)
            {
                return Get<T>(key);
            }

            return default(T);
        }
    }
}
