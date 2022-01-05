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
using ToBot.Common.Data.Interfaces;

namespace ToBot.App
{
    public  class Statistics
        : IObjectId
    {
        private static readonly object SyncObject = new object();
        private static Statistics _instance;

        private Statistics()
        {
            IdObject = Guid.NewGuid().ToString();
            CreationTime = DateTime.Now;
        }

        [BsonIgnore]
        public static Statistics Instance { get { return GetInstance(false); } }

        [BsonIgnore]
        public string DisplayString { get { return $"Uptime: {Uptime:G}, Start time: {StartTime:yyyy-MM-dd HH:mm:ss}, Creation time: {CreationTime:yyyy-MM-dd HH:mm:ss}, Exceptions: {Exceptions}, Commands calls: {CommandsCalls}"; } }

        [BsonIgnore]
        public TimeSpan Uptime { get { return DateTime.Now - StartTime; } }

        [BsonId(true)]
        public string IdObject { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime StartTime { get; set; }

        public long Exceptions { get; set; }

        public long CommandsCalls { get; set; }

        private static Statistics GetInstance(bool recreate)
        {
            if (_instance == null || recreate)
            {
                lock (SyncObject)
                {
                    if (_instance == null || recreate)
                    {
                        _instance = new Statistics();
                    }
                }
            }

            return _instance;
        }

        public void Clear()
        {
            DateTime startTime = StartTime;

            Statistics instance = GetInstance(true);

            instance.StartTime = startTime;
            instance.Exceptions = 0;
            instance.CommandsCalls = 0;
        }

        public Statistics Copy()
        {
            return new Statistics()
            {
                IdObject = IdObject,
                CreationTime = CreationTime,
                StartTime = StartTime,
                Exceptions = Exceptions,
                CommandsCalls = CommandsCalls
            };
        }
    }
}
