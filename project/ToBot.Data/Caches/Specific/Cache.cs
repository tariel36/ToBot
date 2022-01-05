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
using ToBot.Data.Caches.CachingObjects;

namespace ToBot.Data.Caches.Specific
{
    public class Cache
        : ICache
    {
        private readonly object _syncObject = new object();
        private readonly Dictionary<Type, Dictionary<CacheKey, object>> _cachedItems;

        public Cache()
        {
            _cachedItems = new Dictionary<Type, Dictionary<CacheKey, object>>();
        }

        #region Manage

        public void Set(CacheKey key, object item)
        {
            lock (_syncObject)
            {
                InternalSet(key, item);
            }
        }

        public List<object> Get(params CacheKey[] keys)
        {
            List<object> result = new List<object>();

            lock (_syncObject)
            {
                result.AddRange(InternalGet(keys));
            }

            return result;
        }

        public object Get(CacheKey key)
        {
            object result ;

            lock (_syncObject)
            {
                result = InternalGet(key);
            }

            return result;
        }

        public List<object> Get(CachePredicate predicate)
        {
            List<object> result;

            lock (_syncObject)
            {
                result = InternalGet(predicate);
            }

            return result;
        }

        public object Remove(CacheKey key)
        {
            object result;

            lock (_syncObject)
            {
                result = InternalRemove(key);
            }

            return result;
        }

        public List<object> Remove(CachePredicate cachePredicate)
        {
            List<object> result;

            lock (_syncObject)
            {
                result = InternalRemove(cachePredicate);
            }

            return result;
        }

        public bool Contains(CacheKey key)
        {
            bool result;

            lock (_syncObject)
            {
                result = ContainsInternal(key);
            }

            return result;
        }

        #endregion

        #region Manage internal

        private void InternalSet(CacheKey key, object item)
        {
            if (item == null)
            {
                return;
            }

            Type typeObj = item.GetType();
            if (!_cachedItems.ContainsKey(typeObj))
            {
                _cachedItems.Add(typeObj, new Dictionary<CacheKey, object>());
            }
            
            _cachedItems[typeObj][key] = item;
        }
        
        private List<object> InternalGet(params CacheKey[] keys)
        {
            List<object> result = new List<object>(keys?.Length ?? 0);

            if (keys != null)
            {
                foreach (CacheKey key in keys)
                {
                    object item = InternalGet(key);
                    if (item != null)
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        private object InternalGet(CacheKey key)
        {
            object result = null;

            if (ContainsInternal(key))
            {
                result = _cachedItems[key.ObjectType][key];
            }

            return result;
        }

        private List<object> InternalGet(CachePredicate predicate)
        {
            List<object> result = null;

            if (ContainsInternal(predicate.ObjectType))
            {
                result = _cachedItems[predicate.ObjectType].Values.Where(predicate.Predicate).ToList();
            }

            return result;
        }

        private object InternalRemove(CacheKey key)
        {
            object result = null;

            if (ContainsInternal(key))
            {
                result = InternalGet(key);
                _cachedItems[key.ObjectType].Remove(key);
            }

            return result;
        }

        private List<object> InternalRemove(CachePredicate predicate)
        {
            List<object> result = new List<object>();
            
            if (ContainsInternal(predicate.ObjectType))
            {
                Dictionary<CacheKey, object> typeDict = _cachedItems[predicate.ObjectType];
                List<KeyValuePair<CacheKey, object>> cachedItems = _cachedItems[predicate.ObjectType].Where(x => predicate.Predicate(x.Value)).ToList();

                foreach (KeyValuePair<CacheKey, object> cachedItem in cachedItems)
                {
                    typeDict.Remove(cachedItem.Key);
                    result.Add(cachedItem.Value);
                }
            }

            return result;
        }

        private bool ContainsInternal(CacheKey key)
        {
            return ContainsInternal(key.ObjectType) && _cachedItems[key.ObjectType].ContainsKey(key);
        }

        private bool ContainsInternal(Type keyType)
        {
            return keyType != null && _cachedItems.ContainsKey(keyType);
        }

        #endregion
    }
}
