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
using System.Linq;

namespace ToBot.Common.Attributes
{
    public sealed class IsCommandAttribute : Attribute
    {
        public IsCommandAttribute()
        {
            Arguments = new Argument[0];
        }

        public IsCommandAttribute(string[] args)
        {
            Arguments = args.Select(x =>
            {
                string[] parts = x.Split(';');

                bool isParams = string.Equals(parts[0], nameof(isParams));
                int typeIdx = isParams ? 1 : 0;
                string typeName = parts[typeIdx];
                string argName = parts[typeIdx + 1];
                string defaultValue = string.Equals(argName, parts.Last()) ? null : parts.Last();

                return new Argument(argName, typeName, isParams, defaultValue);
            }).ToArray();
        }

        public Argument[] Arguments { get; }

        public class Argument
        {
            public string Name;
            public string TypeName;
            public bool IsParams;
            public string DefaultValue;

            public Argument(string argName, string argType, bool isParams, string defaultValue)
            {
                Name = argName;
                TypeName = argType;
                IsParams = isParams;
                DefaultValue = defaultValue;
            }
        }
    }
}
