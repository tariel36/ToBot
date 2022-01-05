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

using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using ToBot.Common.Data.Interfaces;
using ToBot.Common.Data.Mapping;
using ToBot.Common.Maintenance.Logging;
using ToBot.Data.DataSources.Specific;
using ToBot.Data.Mapping;

namespace ToBot.Data.DataSources
{
    public class LiteDbDataSource
        : DataSource
    {
        #region ctor

        public LiteDbDataSource(ILogger logger, string dbPath)
        {
            Logger = logger;
            Database = new LiteDatabase(dbPath);
            PropertyMapper = new LiteDbPropertyMapper();
        }

        #endregion

        #region Properties

        public override IPropertyMapper PropertyMapper { get; }

        private ILogger Logger { get; }

        private LiteDatabase Database { get; set; }

        #endregion

        #region IDisposable

        public override void Dispose()
        {
            try
            {
                if (Database != null)
                {
                    Database.Dispose();
                }
            }
            catch (Exception ex)
            {
                App.Statistics.Instance.Exceptions++;
                Logger.LogMessage(LogLevel.Error, nameof(LiteDbDataSource), ex.ToString());
            }
            finally
            {
                Database = null;
            }
        }

        #endregion
        
        #region Get | Set | Delete

        public override int Delete<T>(T item)
        {
            return Database.GetCollection<T>(typeof(T).Name).Delete(x => x.IdObject.Equals(item.IdObject));
        }

        public override int DeleteAll<T>()
        {
            int count = Database.GetCollection<T>(typeof(T).Name).Count();
            Database.DropCollection(typeof(T).Name);
            return count;
        }

        public override T Get<T>(string id)
        {
            return Database.GetCollection<T>(typeof(T).Name).FindOne(x => x.IdObject.Equals(id));
        }

        public override List<T> Get<T>(Func<T, bool> predicate)
        {
            return Database.GetCollection<T>(typeof(T).Name).FindAll().Where(predicate).ToList();
        }

        public override bool Set<T>(T toSet)
        {
            return Database.GetCollection<T>(typeof(T).Name).Upsert(toSet);
        }

        public List<T> GetAll<T>()
            where T : IObjectId
        {
            return Database.GetCollection<T>(typeof(T).Name).FindAll().ToList();
        }

        #endregion
    }
}