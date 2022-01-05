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

using Newtonsoft.Json;
using System;
using ToBot.Common.Maintenance.Logging;

namespace ToBot.Data.Serialization
{
    public class JsonSerializer
    {
        public JsonSerializer(ILogger logger)
        {
            Logger = logger;
        }

        private ILogger Logger { get; }

        public T Deserialize<T>(string content)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (Exception ex)
            {
                App.Statistics.Instance.Exceptions++;
                Logger.LogMessage(LogLevel.Error, nameof(JsonSerializer), ex.ToString());
            }

            return default(T);
        }

        public string Serialize<T>(T obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj);
            }
            catch (Exception ex)
            {
                App.Statistics.Instance.Exceptions++;
                Logger.LogMessage(LogLevel.Error, nameof(JsonSerializer), ex.ToString());
            }

            return null;
        }

        public T DeserializeFromFile<T>(string filePath)
        {
            try
            {
                return Deserialize<T>(System.IO.File.ReadAllText(filePath));
            }
            catch (Exception ex)
            {
                App.Statistics.Instance.Exceptions++;
                Logger.LogMessage(LogLevel.Error, nameof(JsonSerializer), ex.ToString());
            }

            return default(T);
        }

        public void SerializeToFile<T>(string filePath, T obj)
        {
            try
            {
                System.IO.File.WriteAllText(filePath, Serialize(obj));
            }
            catch (Exception ex)
            {
                App.Statistics.Instance.Exceptions++;
                Logger.LogMessage(LogLevel.Error, nameof(JsonSerializer), ex.ToString());
            }
        }
    }
}
