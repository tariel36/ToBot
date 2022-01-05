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
using System.Threading.Tasks;
using ToBot.Common.Events;
using ToBot.Common.Maintenance.Logging;
using ToBot.Common.Pocos;
using ToBot.Communication.Messaging;
using ToBot.Communication.Messaging.Formatters;
using ToBot.Communication.Messaging.Providers;
using ToBot.Data.Repositories;

namespace ToBot.Plugin.Plugins
{
    public abstract class BasePlugin
        : Common.Disposables.IDisposable
    {
        public delegate void NotificationDelegate(object sender, NotificationContext ctx);

        protected BasePlugin(IRepository repository,
            ILogger logger,
            IMessageFormatter messageFormatter,
            IEmoteProvider emoteProvider,
            string commandsPrefix)
        {
            Repository = repository;
            Logger = logger;
            MessageFormatter = messageFormatter;
            EmoteProvider = emoteProvider;
            CommandsPrefix = commandsPrefix;

            NotificationEvent = new SafeEvent<NotificationContext>();
        }

        public SafeEvent<NotificationContext> NotificationEvent { get; }

        public bool IsDisposed { get; private set; }

        public virtual string Name { get { return Type.Name; } }

        public virtual Version Version { get { return Type.Assembly.GetName().Version; } }

        public virtual Type Type { get { return GetType(); } }

        protected ILogger Logger { get; }

        protected IMessageFormatter MessageFormatter { get; }

        protected IEmoteProvider EmoteProvider { get; }

        protected IRepository Repository { get; }

        private string CommandsPrefix { get; }

        public abstract void Start();

        public abstract void Stop();

        public virtual void Dispose()
        {
            IsDisposed = true;
        }

        public virtual async Task GuildMemberAdded(ulong id, RespondStringAsyncDelegate messageDelegate)
        {
            await Task.CompletedTask;
        }

        protected void OnNotification(NotificationContext ctx)
        {
            if (NotificationEvent != null)
            {
                NotificationEvent.Invoke(this, ctx);
            }
        }
    }
}
