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
using System.Text;

namespace ToBot.Common.Pocos
{
    public class GuildMember
    {
        public ulong Id { get; set; }

        public string Discriminator { get; set; }

        public string UserName { get; set; }

        public string DisplayName { get; set; }

        public string Nickname { get; set; }

        public string[] Roles { get; set; }

        public string FullUserName { get { return $"{UserName}#{Discriminator}"; } }

        public string Name
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(DisplayName)) { return DisplayName; }
                if (!string.IsNullOrWhiteSpace(Nickname)) { return Nickname; }

                return FullUserName;
            }
        }

        public string[] GetNames()
        {
            return new string[]
            {
                Discriminator,
                UserName,
                DisplayName,
                Nickname,
                FullUserName,
                Name
            };
        }
    }
}
