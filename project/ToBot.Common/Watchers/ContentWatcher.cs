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
using System.Threading;
using System.Threading.Tasks;
using ToBot.Common.Extensions;
using ToBot.Common.Maintenance;
using ToBot.Common.Maintenance.Logging;

namespace ToBot.Common.Watchers
{
    public class ContentWatcher
        : Disposables.IDisposable
    {
        public delegate void ContentWatcherActionDelegate(ContentWatcherContext ctx);

        public ContentWatcher(ILogger logger,
            ExceptionHandler exceptionHandler,
            ContentWatcherActionDelegate watcherAction,
            TimeSpan waitTimeout,
            string name)
        {
            Logger = logger ?? throw new ArgumentException(nameof(logger));
            ExceptionHandler = exceptionHandler ?? throw new ArgumentException(nameof(exceptionHandler));
            WatcherAction = watcherAction ?? throw new ArgumentException(nameof(watcherAction));
            WaitTimeout = waitTimeout;
            Name = name;
        }
        
        public bool IsRunning { get { return Task != null && !Task.IsCompleted; } }

        public bool IsDisposed { get; private set; }

        public string Name { get; }

        private ExceptionHandler ExceptionHandler { get; }

        private ContentWatcherActionDelegate WatcherAction { get; }

        private ILogger Logger { get; }

        private TimeSpan WaitTimeout { get; }

        private Task Task { get; set; }

        private CancellationTokenSource CancellationTokenSource { get; set; }

        public void Start()
        {
            try
            {
                if (IsRunning)
                {
                    return;
                }

                CancellationTokenSource.SafeDispose(ExceptionHandler);
                CancellationTokenSource = new CancellationTokenSource();


                Task = new Task(TaskProcedure, null, CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
                Task.ContinueWith(LogTaskFinished, CancellationTokenSource.Token);
                Task.Start(TaskScheduler.Default);

                Logger.LogMessage(LogLevel.Info, nameof(ContentWatcher), $"Task `{Name}` has started");
            }
            catch (Exception ex)
            {
                ExceptionHandler.OnException(ex);
            }
        }

        public void Stop()
        {
            try
            {
                CancellationTokenSource?.Cancel();
                Task = null;
            }
            catch (Exception ex)
            {
                ExceptionHandler.OnException(ex);
            }
        }

        public void Dispose()
        {
            Stop();

            CancellationTokenSource.SafeDispose(ExceptionHandler);

            IsDisposed = true;
        }

        public void ExecuteProcedure()
        {
            TaskProcedure(new object[] { CancellationTokenSource.Token });
        }

        private void TaskProcedure(object state)
        {
            try
            {
                object[] stateArray = state as object[] ?? new object[0];

                CancellationToken token = stateArray.TryGetValue<CancellationToken>(0);
                int waitMilliseconds = (int) WaitTimeout.TotalMilliseconds;

                while (!token.IsCancellationRequested)
                {
                    Task.Delay(waitMilliseconds, token).Wait(token);

                    if (token.IsCancellationRequested)
                    {
                        Logger.LogMessage(LogLevel.Warning, nameof(ContentWatcher), $"Task of {nameof(ContentWatcher)} with name `{Name}` has been canceled.");
                        return;
                    }

                    try
                    {
                        WatcherAction(new ContentWatcherContext()
                        {
                            Token = token
                        });
                    }
                    catch (Exception iex)
                    {
                        ExceptionHandler.OnException(iex);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.OnException(ex);
            }
        }

        private void LogTaskFinished(Task obj)
        {
            try
            {
                Logger.LogMessage(LogLevel.Warning, nameof(ContentWatcher), $"Task of {nameof(ContentWatcher)} with name `{Name}` has finished with status `{obj.Status}`");
            }
            catch (Exception ex)
            {
                ExceptionHandler.OnException(ex);
            }
        }
    }
}
