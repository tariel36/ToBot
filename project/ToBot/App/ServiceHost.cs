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
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using ToBot.Common.Maintenance;
using ToBot.Common.Maintenance.Logging;
using ToBot.Common.Maintenance.Logging.Writers.Specific;
using ToBot.Common.Pocos;
using ToBot.Communication.Messaging.Formatters;
using ToBot.Communication.Messaging.Providers;
using ToBot.Data.Caches.Specific;
using ToBot.Data.DataSources;
using ToBot.Data.Providers;
using ToBot.Data.Repositories;
using ToBot.Data.Repositories.Specific;
using ToBot.Discord;
using ToBot.Plugin.Plugins;
using ToBot.Plugins;

namespace ToBot.App
{
    public class ServiceHost
    {
        private const string DefaultConfigFilePath = @"config.json";

        public ServiceHost(string[] args)
        {
            Args = args;

            ArgumentsParser parser = new ArgumentsParser();

            ConfigFilePath = parser.GetConfigValue(args, "-c", DefaultConfigFilePath);
            ModuleNames = parser.GetConfigValues<string>(args, "-m");
            RootPath = parser.GetConfigValue<string>(args, "-sp");
            NetStandardDllPath = parser.GetConfigValue<string>(args, "-ns");
        }

        private string[] Args { get; set; }

        private string RootPath { get; set; }

        private string NetStandardDllPath { get; set; }

        private string ConfigFilePath { get; set; }

        private string[] ModuleNames { get; set; }

        private IRepository Repository { get; set; }

        private ILogger Logger { get; set; }

        private IMessageFormatter MessageFormatter { get; set; }

        private IEmoteProvider EmoteProvider { get; set; }

        private ExceptionHandler ExceptionHandler { get; set; }

        private PluginsManager PluginsManager { get; set; }

        private PluginLoader Loader { get; set; }

        private PluginNotificationQueue NotificationQueue { get; set; }

        private CancellationTokenSource CancellationTokenSource { get; set; }

        private ManualResetEvent CloseEvent { get; set; }

        private ManualResetEvent ReconnectEvent { get; set; }

        private DiscordClient DiscordClient { get; set; }

        private CommandsNextModule CommandsNext { get; set; }

        private DiscordEventsHandler DiscordEventsHandler { get; set; }

        public void Start()
        {
            Logger = new Common.Maintenance.Logging.Specific.Logger().Add(new ConsoleWriter());
            ExceptionHandler = new ExceptionHandler(Logger);
            PluginsManager = new PluginsManager(Logger);
            Loader = new PluginLoader(Logger);
            MessageFormatter = new DiscordMessageFormatter();
            EmoteProvider = new DiscordEmotesProvider();

            InitializeApp();

            InitializeDiscord();

            NotificationQueue = new PluginNotificationQueue(Logger, DiscordClient, ExceptionHandler, MessageFormatter);

            RegisterPlugins();

            Start(null);
        }

        public void Stop()
        {
            CloseEvent.Set();
            Repository.Dispose();
        }

        private void InitializeApp()
        {
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomainOnFirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            CloseEvent = new ManualResetEvent(false);
            ReconnectEvent = new ManualResetEvent(false);

            Config.CurrentConfig = new JsonConfigProvider(Logger).Get(ConfigFilePath) ?? new Config();
            Config.CurrentConfig.LogFilePath = $"{Config.CurrentConfig.LogsDirectory}/{DateTime.Now.Date:yyyyMMdd}.txt";

            Logger.Add(new FileWriter(ExceptionHandler, Config.CurrentConfig.LogFilePath));

            if (!string.IsNullOrWhiteSpace(Config.CurrentConfig.LogsDirectory) && !Directory.Exists(Config.CurrentConfig.LogsDirectory))
            {
                Directory.CreateDirectory(Config.CurrentConfig.LogsDirectory);
            }

            Repository = new Repository(Logger, new Cache(), new LiteDbDataSource(Logger, Config.CurrentConfig.DatabaseFilePath));

            Statistics.Instance.StartTime = DateTime.Now;
        }

        private async void Start(Task prevTask)
        {
            Logger.LogMessage(Common.Maintenance.Logging.LogLevel.Error, nameof(Start), $"Starting bot");

            if (prevTask != null)
            {
                Logger.LogMessage(Common.Maintenance.Logging.LogLevel.Error, nameof(Start), $"Previous task exists. IsCanceled: {prevTask.IsCanceled}, IsCompleted: {prevTask.IsCompleted}, IsFaulted: {prevTask.IsFaulted}, Status: {prevTask.Status}");

                if (prevTask.IsCanceled && !ReconnectEvent.WaitOne(TimeSpan.Zero))
                {
                    Logger.LogMessage(Common.Maintenance.Logging.LogLevel.Error, nameof(Start), $"Task cancelled task, closing bot.");
                    CloseEvent.Set();
                    return;
                }

                ReconnectEvent.Reset();
            }

            await MainAsync().ContinueWith(t => Start(t));
        }

        private async Task MainAsync()
        {
            Logger.LogMessage(Common.Maintenance.Logging.LogLevel.Info, nameof(MainAsync), $"{nameof(MainAsync)} started!");

            Logger.LogMessage(Common.Maintenance.Logging.LogLevel.Info, nameof(MainAsync), $"{nameof(MainAsync)} Arguments: {(Args?.Length.ToString() ?? "<null>")}");

            if (CancellationTokenSource != null)
            {
                try
                {
                    CancellationTokenSource.Dispose();
                }
                catch (Exception ex)
                {
                    Statistics.Instance.Exceptions++;
                    Logger.LogMessage(Common.Maintenance.Logging.LogLevel.Error, nameof(MainAsync), $"Failed to dispose {nameof(CancellationTokenSource)}. {ex}");
                }
            }

            CancellationTokenSource = new CancellationTokenSource();

            await DiscordClient.ConnectAsync();
            await Task.Delay(-1, CancellationTokenSource.Token);
        }

        private void InitializeDiscord()
        {
            try
            {
                if (DiscordClient != null)
                {
                    DiscordClient.GuildMemberAdded -= DiscordClient_GuildMemberAdded;
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(Common.Maintenance.Logging.LogLevel.Error, nameof(MainAsync), $"Failed to unsubscirbe event from old discord client. {ex}");
            }

            DiscordClient = new DiscordClient(Config.CurrentConfig.ToDiscordConfiguration());

            DiscordEventsHandler = new DiscordEventsHandler(Logger, Reconnect, DiscordClient);

            CommandsNext = DiscordClient.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = Config.CurrentConfig.CommandPrefix,
                CaseSensitive = false,
                EnableDefaultHelp = true,
                Dependencies = new DependencyCollectionBuilder()
                    .AddInstance(PluginsManager)
                    .Build()
            });

            DiscordClient.UseInteractivity(new InteractivityConfiguration());

            DiscordClient.GuildMemberAdded += DiscordClient_GuildMemberAdded;
        }

        private void RegisterPlugins()
        {
            ICollection<PluginInfo> plugins = Loader.Load(NetStandardDllPath, RootPath, ModuleNames);

            foreach (string moduleName in ModuleNames)
            {
                PluginInfo pluginInfo = plugins.FirstOrDefault(x => string.Equals(x.Name, moduleName, StringComparison.InvariantCultureIgnoreCase));
                if (pluginInfo != null)
                {
                    BasePlugin instance = (BasePlugin) Activator.CreateInstance(pluginInfo.PluginType, Repository, Logger, MessageFormatter, EmoteProvider, Config.CurrentConfig.CommandPrefix, ExceptionHandler);
                    PluginsManager.RegisterPlugin(instance, pluginInfo.CommandsType, CommandsNext.RegisterCommands, PluginOnNotification);
                }
            }
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            Logger.LogMessage(Common.Maintenance.Logging.LogLevel.Error, $"{nameof(ServiceHost)}.{nameof(CurrentDomainOnUnhandledException)}", $"IsTerminating: {unhandledExceptionEventArgs.IsTerminating}, {((unhandledExceptionEventArgs.ExceptionObject as Exception)?.ToString() ?? unhandledExceptionEventArgs.ExceptionObject?.ToString() ?? "<null>")}");
        }

        private void CurrentDomainOnFirstChanceException(object sender, FirstChanceExceptionEventArgs firstChanceExceptionEventArgs)
        {
            Logger.LogMessage(Common.Maintenance.Logging.LogLevel.Error, $"{nameof(ServiceHost)}.{nameof(CurrentDomainOnFirstChanceException)}", firstChanceExceptionEventArgs?.Exception?.ToString() ?? "<null>");
        }

        private void Reconnect()
        {
            string hash = Guid.NewGuid().ToString().Split('-').First();

            Logger.LogMessage(Common.Maintenance.Logging.LogLevel.Debug, $"{nameof(ServiceHost)}.{nameof(Reconnect)}", $"[{hash}] Reconnecting....");

            if (!ReconnectEvent.WaitOne(TimeSpan.Zero))
            {
                Logger.LogMessage(Common.Maintenance.Logging.LogLevel.Debug, $"{nameof(ServiceHost)}.{nameof(Reconnect)}", $"[{hash}] ReconnectEvent is in valid state, SET and CANCEL");

                ReconnectEvent.Set();
                CancellationTokenSource.Cancel();
            }

            Logger.LogMessage(Common.Maintenance.Logging.LogLevel.Debug, $"{nameof(ServiceHost)}.{nameof(Reconnect)}", $"[{hash}] Finished waiting....");
        }

        private void PluginOnNotification(object sender, NotificationContext ctx)
        {
            NotificationQueue.PluginOnNotification(sender, ctx);
        }

        private async Task DiscordClient_GuildMemberAdded(GuildMemberAddEventArgs args)
        {
            Logger.LogMessage(Common.Maintenance.Logging.LogLevel.Debug, $"{nameof(ServiceHost)}.{nameof(Reconnect)}", $"New guild member `{args.Member.DisplayName}` joined server `{args.Guild.Name}`");

            await PluginsManager.GuildMemberAdded(DiscordClient, args.Member.Id);
        }
    }
}
