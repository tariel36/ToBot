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

using CodeHollow.FeedReader;
using System;
using System.Collections.Generic;
using System.Linq;
using ToBot.Rss.Enums;
using ToBot.Rss.Pocos;

namespace ToBot.Rss.ExternalLibraries.Converters.CodeHollow.FeedReader
{
    internal class FeedsToEntriesConverter
    {
        public RssEntry ToRssEntry(FeedItem item)
        {
            if (item == null)
            {
                return null;
            }

            RssEntry entry = new RssEntry();

            entry.Author = item.Author;
            entry.Categories = item.Categories?.ToArray();
            entry.Content = item.Content;
            entry.Description = item.Description;
            entry.Id = item.Id;
            entry.Link = item.Link;
            entry.PublishDate = item.PublishingDate;
            entry.Title = item.Title;

            return entry;
        }

        public RssEntryCollection ToRssEntryCollection(Feed feed)
        {
            if (feed == null)
            {
                return null;
            }

            List<RssEntry> entries = new List<RssEntry>();

            RssEntryCollection collection = new RssEntryCollection();
            collection.Copyright = feed.Copyright;
            collection.Description = feed.Description;
            collection.ImageUrl = feed.ImageUrl;
            collection.Language = feed.Language;
            collection.LastUpdatedDate = feed.LastUpdatedDate;
            collection.Link = feed.Link;
            collection.Title = feed.Title;
            collection.Type = ToFeedTypes(feed.Type);
            collection.Copyright = feed.Copyright;
            collection.Entries = entries;

            if (feed.Items?.Count > 0)
            {
                foreach (FeedItem item in feed.Items)
                {
                    RssEntry entry = ToRssEntry(item);

                    if (entry != null)
                    {
                        entries.Add(entry);
                    }
                }
            }

            return collection;
        }

        public FeedTypes ToFeedTypes(FeedType type)
        {
            switch (type)
            {
                case FeedType.Atom: return FeedTypes.Atom;
                case FeedType.Rss_0_91: return FeedTypes.Rss091;
                case FeedType.Rss_0_92: return FeedTypes.Rss092;
                case FeedType.Rss_1_0: return FeedTypes.Rss10;
                case FeedType.Rss_2_0: return FeedTypes.Rss20;
                case FeedType.MediaRss: return FeedTypes.MediaRss;
                case FeedType.Rss: return FeedTypes.Rss;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
