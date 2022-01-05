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
using System.Text;
using ToBot.Communication.Messaging.Formatters;

namespace ToBot.Discord
{
    public class DiscordMessageFormatter
        : IMessageFormatter
    {
        public StringBuilder MultilineBlock(StringBuilder sb)
        {
            sb.Insert(0, "```");
            sb.Append("```");

            return sb;
        }

        public string Mention(ulong idUser)
        {
            return $"<@{idUser}>";
        }

        public StringBuilder Block(StringBuilder sb)
        {
            sb.Insert(0, "`");
            sb.Append("`");

            return sb;
        }

        public string Block(string str)
        {
            return $"`{str}`";
        }

        public string NoEmbed(string str)
        {
            return $"<{str}>";
        }

        public string Bold(string str)
        {
            return $"**{str}**";
        }

        public string Strike(string str)
        {
            return $"~~{str}~~";
        }

        public List<string> SplitMessage(string msg)
        {
            List<string> result = new List<string>();

            if (!string.IsNullOrWhiteSpace(msg))
            {
                if (msg.Length > DiscordConsts.MaxMessageLength)
                {
                    StringBuilder sb = new StringBuilder();

                    string[] parts = msg.Split(' ');

                    for (int i = 0; i < parts.Length; ++i)
                    {
                        string part = parts[i].Trim();

                        if (part.Length >= DiscordConsts.MaxMessageLength)
                        {
                            for (int j = 0; j < part.Length; j += DiscordConsts.MaxMessageLength)
                            {
                                result.Add(part.Substring(j, DiscordConsts.MaxMessageLength));
                            }
                        }
                        else if ((sb.Length + part.Length + 1) < DiscordConsts.MaxMessageLength)
                        {
                            if (i + 1 <= parts.Length)
                            {
                                sb.Append(' ');
                            }

                            sb.Append(part);
                        }
                        else
                        {
                            --i;
                            result.Add(sb.ToString().Trim());
                            sb.Clear();
                        }
                    }

                    if (sb.Length > 0)
                    {
                        result.Add(sb.ToString().Trim());
                    } 
                }
                else
                {
                    result.Add(msg);
                }
            }

            return result;
        }
    }
}
