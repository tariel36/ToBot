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
using ToBot.Common.Maintenance.Logging;

namespace ToBot.Common.Maintenance
{
    public static class SafeExecution
    {
        public static T Execute<T>(ILogger logger, Func<T> func)
        {
            return Execute(logger, func, out _);
        }

        public static T Execute<T>(ILogger logger, Func<T> func, out Exception error)
        {
            T result = default(T);
            error = null;

            try
            {
                result = func();
            }
            catch (Exception ex)
            {
                error = ex;
                logger.LogMessage(LogLevel.Error, nameof(SafeExecution), ex.ToString());
            }

            return result;
        }

        public static void Execute(ILogger logger, Action action)
        {
            Execute(logger, action, out _);
        }

        public static void Execute(ILogger logger, Action action, out Exception error)
        {
            error = null;

            try
            {
                action();
            }
            catch (Exception ex)
            {
                error = ex;
                logger.LogMessage(LogLevel.Error, nameof(SafeExecution), ex.ToString());
            }
        }
    }
}
