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
using ToBot.Common.Data.Interfaces;
using ToBot.Common.Data.Mapping;
using ToBot.Common.Extensions;
using ToBot.Common.Maintenance.Logging;
using ToBot.Data.Caches;
using ToBot.Data.Caches.CachingObjects;
using ToBot.Data.DataSources;
using ToBot.Data.Repositories.Invocators;
using ToBot.Data.Repositories.Invocators.Specific;

namespace ToBot.Data.Repositories.Specific
{
    public class Repository
        : IRepository
    {
        private readonly object _syncLock = new object();

        public Repository(ILogger logger,
            ICache cache,
            IDataSource dataSource)
        {
            Logger = logger;
            Cache = cache;
            Source = dataSource;

            InvocatorsDict = new Dictionary<Type, ISourceInvocator>();
        }

        public bool IsDisposed { get; private set; }

        public IPropertyMapper PropertyMapper { get { return Source.PropertyMapper; } }

        private ILogger Logger { get; }

        private ICache Cache { get; }

        private IDataSource Source { get; }

        private Dictionary<Type, ISourceInvocator> InvocatorsDict { get; }

        #region IDisposable

        public void Dispose()
        {
            Source.SafeDispose();

            IsDisposed = true;
        }

        private void TryDispose(IDisposable disposable, Action dispose)
        {
            try
            {
                if (disposable != null)
                {
                    dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(LogLevel.Error, nameof(Repository), ex.ToString());
            }
        }

        #endregion

        #region Invocators

        public void AddInvocator<T>(ISourceInvocator<T> invocator)
        {
            lock (_syncLock)
            {
                Type type = typeof(T);

                if (!InvocatorsDict.ContainsKey(type))
                {
                    InvocatorsDict[type] = invocator; 
                }
            }
        }

        public void AddInvocator<T>()
            where T : IObjectId
        {
            AddInvocator(new SourceInvocator<T>(Source));
        }

        public void RemoveInvocator<T>()
        {
            lock (_syncLock)
            {
                InvocatorsDict.Remove(typeof(T));
            }
        }

        #endregion

        #region SetItem | TryGetItem | DeleteItem

        public int DeleteAll<T>()
            where T : IObjectId
        {
            lock (_syncLock)
            {
                ISourceInvocator<T> invocator;

                if (TryGetInvocator(out invocator))
                {
                    Cache.Remove(CachePredicate.Create<T>(x => true));
                    return invocator.DeleteAll();
                }

                return 0;
            }
        }

        public int DeleteItem<T>(T item)
            where T : IObjectId
        {
            lock (_syncLock)
            {
                ISourceInvocator<T> invocator;

                if (TryGetInvocator(out invocator))
                {
                    Cache.Remove(CacheKey.Create<T>(item.IdObject));
                    return invocator.Delete(item);  
                }

                return 0;
            }
        }

        public bool SetItem<T>(T item)
            where T : IObjectId
        {
            lock (_syncLock)
            {
                return InnerSetItem(item);
            }
        }

        public List<T> TryGetItem<T>(string[] ids)
            where T : IObjectId
        {
            lock (_syncLock)
            {
                List<T> result = new List<T>();

                ISourceInvocator<T> invocator;

                if (TryGetInvocator(out invocator))
                {
                    if (ids != null && ids.Length > 0)
                    {
                        ids = ids.Distinct().ToArray();

                        result = Cache.Get(ids.Select(x => x).Select(CacheKey.Create<T>).ToArray())
                            .Where(x => x is T)
                            .Cast<T>()
                            .ToList();

                        if (result.Count != ids.Length)
                        {
                            string[] toGet = ids.Except(result.Select(x => x.IdObject)).ToArray();

                            if (toGet.Length > 0)
                            {
                                List<T> items = invocator.Get(o => toGet.Contains(o.IdObject));
                                foreach (T item in items)
                                {
                                    Cache.Set(CacheKey.Create<T>(item.IdObject), item);
                                    result.Add(item);
                                }
                            }
                        }
                    } 
                }

                return result; 
            }
        }

        public List<T> TryGetItems<T>(Func<T, bool> predicate)
            where T : IObjectId
        {
            lock (_syncLock)
            {
                List<T> result = new List<T>();

                if (predicate != null)
                {
                    ISourceInvocator<T> invocator;

                    if (TryGetInvocator(out invocator))
                    {
                        List<T> items = invocator.Get(predicate);

                        if (items != null && items.Count > 0)
                        {
                            foreach (T item in items)
                            {
                                InnerSetItem(item);
                                result.Add(item);
                            }
                        } 
                    }
                }

                return result; 
            }
        }

        #endregion

        #region Inner GET | SET | DELETE

        private bool InnerSetItem<T>(T item)
            where T : IObjectId
        {
            bool result = false;

            ISourceInvocator<T> invocator;

            if (TryGetInvocator(out invocator))
            {
                result = invocator.Set(item);

                if (result)
                {
                    Cache.Set(CacheKey.Create<T>(item.IdObject), item);
                }
            }

            return result;
        }

        #endregion

        #region Invocator GET | SET

        private bool TryGetInvocator<T>(out ISourceInvocator<T> invocator)
        {
            Type type = typeof(T);

            invocator = default(ISourceInvocator<T>);

            if (InvocatorsDict.ContainsKey(type))
            {
                invocator = InvocatorsDict[type] as ISourceInvocator<T>;
            }

            return invocator != default(ISourceInvocator<T>);
        }

        #endregion
    }
}
