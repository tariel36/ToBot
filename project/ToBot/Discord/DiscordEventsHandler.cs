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
using DSharpPlus.EventArgs;
using System;
using System.Threading.Tasks;
using ToBot.Common.Maintenance.Logging;
using ToBot.Maintenance;

namespace ToBot.Discord
{
    public sealed class DiscordEventsHandler
    {
        public DiscordEventsHandler(ILogger logger, Action reconnect, DiscordClient client)
        {
            Logger = logger;
            Reconnect = reconnect;
            Client = client;

            Client.SocketClosed += ClientOnSocketClosed;
            Client.SocketErrored += ClientOnSocketErrored;
            Client.ClientErrored += ClientOnClientErrored;
            Client.SocketOpened += ClientOnSocketOpened;
            Client.MessageCreated += OnMessageCreated;
            Client.DebugLogger.LogMessageReceived += DebugLoggerOnLogMessageReceived;
        }

        private ILogger Logger { get; }

        private Action Reconnect { get; }

        private DiscordClient Client { get; }

        private void DebugLoggerOnLogMessageReceived(object sender, DebugLogMessageEventArgs e)
        {
            Logger.LogMessage(LogLevelConverter.Convert(e.Level), e.Application, e.Message);
        }

        private Task OnMessageCreated(MessageCreateEventArgs e)
        {
            Logger.LogMessage(Common.Maintenance.Logging.LogLevel.Debug, $"{nameof(Program)}.{nameof(OnMessageCreated)}", $"{e.Author.Username}: {e.Message.Content}");
            return Task.CompletedTask;
        }

        private Task ClientOnSocketOpened()
        {
            Logger.LogMessage(Common.Maintenance.Logging.LogLevel.Debug, $"{nameof(Program)}.{nameof(ClientOnSocketOpened)}", nameof(ClientOnSocketOpened));
            return Task.CompletedTask;
        }

        private Task ClientOnClientErrored(ClientErrorEventArgs clientErrorEventArgs)
        {
            Logger.LogMessage(Common.Maintenance.Logging.LogLevel.Error, $"{nameof(Program)}.{nameof(ClientOnClientErrored)}", $"{clientErrorEventArgs.EventName} {clientErrorEventArgs.Exception}");
            return Task.CompletedTask;
        }

        private Task ClientOnSocketErrored(SocketErrorEventArgs socketErrorEventArgs)
        {
            Logger.LogMessage(Common.Maintenance.Logging.LogLevel.Error, $"{nameof(Program)}.{nameof(ClientOnSocketErrored)}", socketErrorEventArgs.Exception.ToString());
            return Task.CompletedTask;
        }

        private Task ClientOnSocketClosed(SocketCloseEventArgs socketCloseEventArgs)
        {
            Logger.LogMessage(Common.Maintenance.Logging.LogLevel.Debug, $"{nameof(Program)}.{nameof(ClientOnSocketClosed)}", $"{((System.Net.WebSockets.WebSocketCloseStatus)socketCloseEventArgs.CloseCode)} ({socketCloseEventArgs.CloseCode}), {socketCloseEventArgs.CloseMessage}");

            Reconnect();

            return Task.CompletedTask;
        }
    }
}
