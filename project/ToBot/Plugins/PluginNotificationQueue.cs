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

using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ToBot.Common.Extensions;
using ToBot.Common.Maintenance;
using ToBot.Common.Maintenance.Logging;
using ToBot.Common.Maintenance.Logging.Specific;
using ToBot.Common.Pocos;
using ToBot.Common.Watchers;
using ToBot.Communication.Messaging.Formatters;
using LogLevel = ToBot.Common.Maintenance.Logging.LogLevel;

namespace ToBot.Plugins
{
    public class PluginNotificationQueue
        : Common.Disposables.IDisposable
    {
        public PluginNotificationQueue(ILogger logger,
            DiscordClient discordClient,
            ExceptionHandler exceptionHandler,
            IMessageFormatter messageFormatter)
        {
            Logger = logger;
            DiscordClient = discordClient;
            MessageFormatter = messageFormatter;

            ContextQueue = new ConcurrentQueue<NotificationContext>();

            Watcher = new ContentWatcher(logger, exceptionHandler, ProcessNotifications, TimeSpan.FromMinutes(1.0), nameof(PluginNotificationQueue));
            Watcher.Start();
        }

        public bool IsDisposed { get; private set; }

        private ILogger Logger { get; }

        private DiscordClient DiscordClient { get; }

        private ConcurrentQueue<NotificationContext> ContextQueue { get; }

        private ContentWatcher Watcher { get; }

        private IMessageFormatter MessageFormatter { get; }

        public void PluginOnNotification(object sender, NotificationContext ctx)
        {
            ContextQueue.Enqueue(ctx);
            Logger.LogMessage(LogLevel.Info, nameof(PluginNotificationQueue), $"Item enqueued. Items count: {ContextQueue.Count}");
        }

        public void Dispose()
        {
            Watcher.Stop();
            Watcher.SafeDispose();
        }

        private void ProcessNotifications(ContentWatcherContext ctx)
        {
            Logger.LogMessage(LogLevel.Info, nameof(PluginNotificationQueue), $"Process notifications. Items count: {ContextQueue.Count}");

            while (ContextQueue.TryDequeue(out NotificationContext notificationCtx))
            {
                List<string> messages = MessageFormatter.SplitMessage(notificationCtx.Message);

                DiscordChannel channel = DiscordClient.GetChannelAsync(notificationCtx.ChannelId).Result;

                if (channel == null)
                {
                    Logger.LogMessage(LogLevel.Warning, nameof(PluginNotificationQueue), $"Channel with id {notificationCtx.ChannelId} not found!"); 
                }
                else
                {
                    foreach (string msg in messages)
                    {
                        channel.SendMessageAsync(msg);
                    } 
                }
            }
        }
    }
}
