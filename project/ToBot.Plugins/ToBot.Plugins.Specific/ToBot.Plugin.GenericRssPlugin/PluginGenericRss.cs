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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ToBot.Common.Attributes;
using ToBot.Common.Extensions;
using ToBot.Common.Maintenance;
using ToBot.Common.Maintenance.Logging;
using ToBot.Common.Pocos;
using ToBot.Common.Watchers;
using ToBot.Communication.Commands;
using ToBot.Communication.Messaging.Formatters;
using ToBot.Communication.Messaging.Providers;
using ToBot.Data.Repositories;
using ToBot.Plugin.GenericRssPlugin.Data;
using ToBot.Plugin.Plugins;
using ToBot.Rss.Clients;
using ToBot.Rss.Pocos;

namespace ToBot.Plugin.GenericRssPlugin
{
    public abstract class PluginGenericRss<TRssEntry>
        : BasePlugin
        where TRssEntry : IRssEntryItem
    {
        public PluginGenericRss(IRepository repository,
            ILogger logger,
            IMessageFormatter messageFormatter,
            IEmoteProvider emoteProvider,
            string commandsPrefix,
            ExceptionHandler exceptionHandler,
            Func<RssEntry, TRssEntry> entryFactory,
            bool appendColonToTitle = true)
            : base(repository, logger, messageFormatter, emoteProvider, commandsPrefix)
        {
            AppendColonToTitle = appendColonToTitle;

            EntryFactory = entryFactory;
            NotificationSubscribers = new ConcurrentDictionary<ulong, SubscribedChannel>();
            Entries = new ConcurrentDictionary<string, TRssEntry>();
            ContentWatcher = new ContentWatcher(logger, exceptionHandler, ContentWatcherProcedure, TimeSpan.FromMinutes(60.0), Name);

            Repository.PropertyMapper
                .Id<TRssEntry, string>(x => x.IdObject, false)
                .Id<SubscribedChannel, string>(x => x.IdObject, false)
                ;

            repository.AddInvocator<SubscribedChannel>();
            repository.AddInvocator<TRssEntry>();
        }

        protected abstract string TaggedUrlWithPageTemplate { get; }

        private ContentWatcher ContentWatcher { get; }

        private ConcurrentDictionary<ulong, SubscribedChannel> NotificationSubscribers { get; }

        private ConcurrentDictionary<string, TRssEntry> Entries { get; }

        private Func<RssEntry, TRssEntry> EntryFactory { get; }

        private bool AppendColonToTitle { get; }

        public override void Start()
        {
            Logger.LogMessage(LogLevel.Debug, Name, $"Starting plugin `{Name}`");

            ContentWatcher.Start();

            List<SubscribedChannel> channels = Repository.TryGetItems<SubscribedChannel>(x => string.Equals(x.Source, Name));

            if (channels?.Count > 0)
            {
                foreach (SubscribedChannel channel in channels)
                {
                    NotificationSubscribers[channel.ChannelId] = channel;
                    Logger.LogMessage(LogLevel.Debug, Name, $"Subscribed channel with id `{channel.ChannelId}` for plugin `{Name}`");
                }
            }
            else
            {
                Logger.LogMessage(LogLevel.Debug, Name, $"No registered channels for plugin `{Name}`");
            }

            foreach (TRssEntry entry in Repository.TryGetItems<TRssEntry>(x => true))
            {
                Entries[entry.WebEntryId ?? entry.IdObject] = entry;
            }

            Logger.LogMessage(LogLevel.Debug, Name, $"Plugin `{Name}` started, subscribed channels count: `{NotificationSubscribers.Count}`, entries count: `{Entries.Count}`");
        }

        public override void Stop()
        {
            ContentWatcher.Stop();
        }

        public override void Dispose()
        {
            ContentWatcher.SafeDispose();

            base.Dispose();
        }

        #region Notify

        [IsCommand]
        public async Task Notify(CommandExecutionContext ctx)
        {
            if (ctx == null)
            {
                return;
            }

            ulong channelId = ctx.Context.ChannelId;

            bool isEnabled = false;

            SubscribedChannel channel;

            if (NotificationSubscribers.ContainsKey(channelId))
            {
                NotificationSubscribers.TryRemove(channelId, out channel);

                Repository.DeleteItem(channel);
            }
            else
            {
                isEnabled = true;
                channel = new SubscribedChannel(channelId, Name);
                NotificationSubscribers.TryAdd(channelId, channel);

                Repository.SetItem(channel);
            }

            string channelName = ctx.Context.ChannelName;

            await ctx.Context.RespondStringAsync($"Notifications are now {(isEnabled ? "enabled" : "disabled")} on {MessageFormatter.Bold(channelName)}.");
        }

        #endregion

        #region Test

        [IsCommand]
        public async Task Test(CommandExecutionContext ctx)
        {
            Logger.LogMessage(LogLevel.Debug, Name, $"Plugin `{Name}` invoked `Test` command.");

            ContentWatcherProcedure(null);
            await Task.CompletedTask;
        }

        #endregion

        private void ContentWatcherProcedure(ContentWatcherContext ctx)
        {
            RssEntriesResult<TRssEntry> newEntriesResult = TryGetAllEntries();

            Logger.LogMessage(LogLevel.Info, nameof(Name), $"New `{Name}` entries {newEntriesResult?.Entries?.Count ?? 0}");

            if (newEntriesResult?.Entries?.Count > 0)
            {
                StringBuilder sb = new StringBuilder(10240);

                sb.AppendLine("New entries have appeared:")
                    .AppendLine();

                foreach (TRssEntry entry in newEntriesResult.Entries)
                {
                    sb.AppendLine($"{entry.Title}{(AppendColonToTitle ? ":" : string.Empty)}")
                        .AppendLine(MessageFormatter.NoEmbed(entry.Link));
                }

                if (newEntriesResult.IsAllNew)
                {
                    sb.AppendLine()
                        .AppendLine("It seems all entries are new, some of them could have been skipped if source page has no pagination enabled.");
                }

                string message = sb.ToString();

                foreach (SubscribedChannel channel in NotificationSubscribers.Values)
                {
                    OnNotification(new NotificationContext() { Message = message, ChannelId = channel.ChannelId });
                }
            }
        }

        private RssEntriesResult<TRssEntry> TryGetAllEntries()
        {
            List<TRssEntry> newEntries = new List<TRssEntry>();
            RssEntryCollection collection;
            RssEntryCollection previousCollection = null;

            bool collectionsEqual = false;

            int page = 1;
            int alreadyExistingCount = 0;

            do
            {
                string url = string.Format(TaggedUrlWithPageTemplate, page++);

                collection = new RssClient().Get(url);

                if (collection?.Entries?.Count > 0)
                {
                    collectionsEqual = collection.CollectionEquals(previousCollection);
                    if (!collectionsEqual)
                    {
                        alreadyExistingCount = 0;

                        foreach (RssEntry entry in collection.Entries)
                        {
                            TRssEntry genericEntry = EntryFactory(entry);

                            if (Entries.ContainsKey(genericEntry.WebEntryId))
                            {
                                alreadyExistingCount++;
                            }
                            else
                            {
                                Repository.SetItem(genericEntry);
                                Entries[genericEntry.WebEntryId] = genericEntry;
                                newEntries.Add(genericEntry);
                            }
                        }
                    }

                    previousCollection = collection;
                }

            } while (collection?.Entries != null && alreadyExistingCount == 0 && !collectionsEqual);

            return new RssEntriesResult<TRssEntry>()
            {
                Entries = newEntries,
                IsAllNew = alreadyExistingCount == 0
            };
        }
    }
}
