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

namespace ToBot.App
{
    public class ArgumentsParser
    {
        public T[] GetConfigValues<T>(string[] args, string name)
        {
            List<T> items = new List<T>();

            for (int i = 0; i < args.Length; ++i)
            {
                string current = args[i];

                if (string.Equals(current, name, StringComparison.InvariantCultureIgnoreCase))
                {
                    ++i;

                    if (i < args.Length)
                    {
                        items.Add(TryGetValue<T>(name, args[i]));
                    }
                }
            }

            return items.ToArray();
        }

        public T GetConfigValue<T>(string[] args, string name, T defaultValue = default(T))
        {
            for (int i = 0; i < args.Length; ++i)
            {
                string current = args[i];

                if (string.Equals(current, name, StringComparison.InvariantCultureIgnoreCase))
                {
                    ++i;

                    if (i >= args.Length)
                    {
                        return defaultValue;
                    }

                    return TryGetValue<T>(name, args[i]);
                }
            }

            return defaultValue;
        }

        private T TryGetValue<T>(string name, string strValue)
        {
            try
            {
                return (T)Convert.ChangeType(strValue, typeof(T));
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to get argument `{name}` value because of conversion exception.", ex);
            }
        }
    }
}
