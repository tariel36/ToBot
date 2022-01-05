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
using System.Linq;
using ToBot.Rss.Enums;

namespace ToBot.Rss.Pocos
{
    public class RssEntryCollection
    {
        public string Copyright { get; internal set; }

        public string Description { get; internal set; }

        public string ImageUrl { get; internal set; }

        public string Language { get; internal set; }

        public DateTime? LastUpdatedDate { get; internal set; }

        public string Link { get; internal set; }

        public string Title { get; internal set; }

        public FeedTypes Type { get; internal set; }

        public ICollection<RssEntry> Entries { get; internal set; }

        public bool CollectionEquals(RssEntryCollection other)
        {
            if (Entries == null && other?.Entries == null)
            {
                return true;
            }

            if (Entries != null && other?.Entries == null
                || (Entries == null && other?.Entries != null))
            {
                return false;
            }

            if (Entries?.Count != other?.Entries?.Count)
            {
                return false;
            }

            if (Entries != null
                && other.Entries != null
                && Entries.Count == other.Entries.Count)
            {
                for (int i = 0; i < Entries.Count; ++i)
                {
                    string id = Entries.ElementAt(i).Id;

                    if (other.Entries.All(x => !string.Equals(x.Id, id)))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }
}
