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
using ToBot.Data.DataSources;

namespace ToBot.Data.Repositories.Invocators.Specific
{
    public class SourceInvocator<T>
        : ISourceInvocator<T>
        where T : IObjectId
    {
        public SourceInvocator()
        {

        }

        public SourceInvocator(IDataSource ds)
            : this(ds.Get, ds.Set, ds.Delete, ds.DeleteAll<T>)
        {

        }

        public SourceInvocator(Func<Func<T, bool>, List<T>> get, Func<T, bool> set, Func<T, int> delete, Func<int> deleteAll)
        {
            Get = get;
            Set = set;
            Delete = delete;
            DeleteAll = deleteAll;
        }

        public Func<T, bool> Set { get; set; }

        public Func<Func<T, bool>, List<T>> Get { get; set; }

        public Func<T, int> Delete { get; set; }

        public Func<int> DeleteAll { get; set; }
    }
}
