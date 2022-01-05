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
using ToBot.Common.Data.Interfaces;
using ToBot.Common.Data.Mapping;

namespace ToBot.Data.DataSources.Specific
{
    public abstract class DataSource
        : IDataSource
    {
        public bool IsDisposed { get; private set; }

        public abstract IPropertyMapper PropertyMapper { get; }

        public abstract int Delete<T>(T item) where T : IObjectId;

        public abstract int DeleteAll<T>() where T : IObjectId;

        public abstract T Get<T>(string id) where T : IObjectId;

        public abstract List<T> Get<T>(Func<T, bool> predicate) where T : IObjectId;

        public abstract bool Set<T>(T toSet) where T : IObjectId;

        public virtual void Dispose()
        {

        }
    }
}
