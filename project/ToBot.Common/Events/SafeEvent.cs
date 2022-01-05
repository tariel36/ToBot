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

namespace ToBot.Common.Events
{
    public sealed class SafeEvent<T>
    {
        private readonly List<EventHandler<T>> _handlers;

        public SafeEvent()
        {
            _handlers = new List<EventHandler<T>>();
        }

        public event EventHandler<T> Event
        {
            add
            {
                InnerEvent += value;
                _handlers.Add(value);
            }
            remove
            {
                InnerEvent -= value;
                _handlers.Remove(value);
            }
        }

        private event EventHandler<T> InnerEvent;

        public static SafeEvent<T> operator +(SafeEvent<T> ev, EventHandler<T> handler)
        {
            ev.Event += handler;
            return ev;
        }

        public static SafeEvent<T> operator -(SafeEvent<T> ev, EventHandler<T> handler)
        {
            ev.Event -= handler;
            return ev;
        }

        public static implicit operator Action<object, T>(SafeEvent<T> ev)
        {
            return ev.Invoke;
        }

        public void Clear()
        {
            while (_handlers.Count > 0)
            {
                Event -= _handlers[0];
            }
        }

        public void Invoke(object sender, T e)
        {
            if (InnerEvent != null)
            {
                InnerEvent(sender, e);
            }
        }

        public Action<object, T> GetInvoker()
        {
            return Invoke;
        }
    }
}
