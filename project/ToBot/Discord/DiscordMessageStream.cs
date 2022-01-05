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

using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToBot.Communication.Messaging.Streams;

namespace ToBot.Discord
{
    public class DiscordMessageStream
        : IMessageStream
    {
        public delegate Task<DiscordMessage> DiscordMessageDelegate(string msg);

        public DiscordMessageStream(DiscordMessageDelegate messageDelegate)
        {
            MessageDelegate = messageDelegate;
        }

        private DiscordMessageDelegate MessageDelegate { get; }

        public async Task Reply(StringBuilder sb)
        {
            await Reply(sb.ToString());
        }

        public async Task Reply(string msg)
        {
            List<string> messages = SplitIntoMessages(msg);

            foreach (string singleMsg in messages)
            {
                await MessageDelegate(singleMsg);
            }
        }

        private List<string> SplitIntoMessages(string msgBuilder)
        {
            List<string> result = new List<string>();

            string[] words = msgBuilder.Split(new [] { " " }, StringSplitOptions.None).ToArray();
            StringBuilder sbSingleMessage = new StringBuilder(msgBuilder.Length);
            for (int i = 0; i < words.Length; ++i)
            {
                string word = words[i];
                if (sbSingleMessage.Length + word.Length > DiscordConsts.MaxMessageLength)
                {
                    result.Add(sbSingleMessage.ToString());
                    sbSingleMessage.Clear();
                }

                sbSingleMessage.Append(word);
                if (i + 1 < words.Length)
                {
                    sbSingleMessage.Append(' ');
                }
            }

            if (sbSingleMessage.Length > 0)
            {
                result.Add(sbSingleMessage.ToString());
                sbSingleMessage.Clear();
            }

            return result;
        }
    }
}
