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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToBot.Common.Attributes;
using ToBot.Common.Data.Interfaces;
using ToBot.Common.Extensions;
using ToBot.Common.Maintenance;
using ToBot.Common.Maintenance.Logging;
using ToBot.Common.Pocos;
using ToBot.Common.Watchers;
using ToBot.Communication.Commands;
using ToBot.Communication.Messaging.Formatters;
using ToBot.Communication.Messaging.Providers;
using ToBot.Data.Repositories;
using ToBot.Plugin.Plugins;

namespace ToBot.Plugin.GenericScrapperPlugin
{
    public class PluginGenericScrapper<TEntry>
        : BasePlugin
        where TEntry : IObjectId
    {
        protected delegate bool NotificationEntryFilter<T>(T entry);
        protected delegate string NotificationEntryMessageFormatter<T>(T entry);

        public PluginGenericScrapper(IRepository repository,
            ILogger logger,
            IMessageFormatter messageFormatter,
            IEmoteProvider emoteProvider,
            string commandsPrefix,
            ExceptionHandler exceptionHandler,
            Func<PluginGenericScrapper<TEntry>, ContentWatcher.ContentWatcherActionDelegate> contentWatcherProcedureProvider,
            TimeSpan? contentWatcherWaitTimeout = null)
            : base(repository, logger, messageFormatter, emoteProvider, commandsPrefix)
        {
            NotificationSubscribers = new ConcurrentDictionary<ulong, SubscribedChannel>();
            ContentWatcher = new ContentWatcher(logger, exceptionHandler, contentWatcherProcedureProvider(this), contentWatcherWaitTimeout ?? TimeSpan.FromHours(1.0), Name);
            Entries = new ConcurrentDictionary<string, TEntry>(StringComparer.InvariantCultureIgnoreCase);
        }

        protected ConcurrentDictionary<ulong, SubscribedChannel> NotificationSubscribers { get; }

        protected ConcurrentDictionary<string, TEntry> Entries { get; }

        private ContentWatcher ContentWatcher { get; }

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

            foreach (TEntry entry in Repository.TryGetItems<TEntry>(x => true))
            {
                Entries[entry.IdObject] = entry;
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

            ContentWatcher.ExecuteProcedure();
            await Task.CompletedTask;
        }

        #endregion

        protected List<string> CreateNotificationMessage<T>(List<T> entries, NotificationEntryFilter<T> filter, NotificationEntryMessageFormatter<T> formatter)
        {
            List<string> items = new List<string>();

            StringBuilder sbMsg = new StringBuilder();

            sbMsg.AppendLine("New articles have appeared:").AppendLine();

            bool atleastOneAdded = false;

            foreach (T entry in entries.Where(x => filter(x)))
            {
                atleastOneAdded = true;

                string line = formatter(entry);

                if (sbMsg.Length + line.Length >= 1900)
                {
                    items.Add(sbMsg.ToString());
                    sbMsg.Clear();
                }

                sbMsg.AppendLine(line);
            }

            items.Add(sbMsg.ToString());

            items.RemoveAll(string.IsNullOrWhiteSpace);

            if (!atleastOneAdded)
            {
                items.Clear();
            }

            return items;
        }

        protected void SendNotifications<T>(List<T> entries, NotificationEntryFilter<T> filter, NotificationEntryMessageFormatter<T> formatter)
        {
            if (entries.Count > 0)
            {
                foreach (string message in CreateNotificationMessage(entries, filter, formatter))
                {
                    foreach (SubscribedChannel channel in NotificationSubscribers.Values)
                    {
                        OnNotification(new NotificationContext() { Message = message, ChannelId = channel.ChannelId });
                    }
                }
            }
        }

        protected bool TruthFilter<T>(T entry)
        {
            return true;
        }
    }
}
