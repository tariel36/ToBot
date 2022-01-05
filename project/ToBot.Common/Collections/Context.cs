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

namespace ToBot.Common.Collections
{
    public class Context<TKey>
    {
        private readonly Dictionary<TKey, object> _items;

        public Context()
        {
            _items = new Dictionary<TKey, object>();
        }

        public void Add(TKey key, object value, bool replace = false)
        {
            if (_items.ContainsKey(key) && !replace)
            {
                throw new InvalidOperationException($"Key {key} already exists and replacing is not allowed!");
            }

            _items[key] = value;
        }

        public object Remove(TKey key)
        {
            object tmp = null;

            if (_items.ContainsKey(key))
            {
                tmp = _items[key];
            }

            _items.Remove(key);

            return tmp;
        }

        public void Delete(TKey key)
        {
            _items.Remove(key);
        }

        public object Get(TKey key)
        {
            return InnerGet(key, true);
        }

        public T TryGet<T>(TKey key, T defaultValue = default(T))
        {
            T result = defaultValue;

            object tmp = InnerGet(key);
            if (tmp is T)
            {
                result = (T)tmp;
            }

            return result;
        }

        public bool ContainsKey(TKey key)
        {
            return _items.ContainsKey(key);
        }

        public void Clear()
        {
            _items.Clear();
        }

        private object InnerGet(TKey key, bool throwException = false)
        {
            if (ContainsKey(key))
            {
                return _items[key];
            }

            if (throwException)
            {
                throw new InvalidOperationException($"No value for key {key}");
            }

            return default(object);
        }
    }
}
