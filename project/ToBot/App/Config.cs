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

using DSharpPlus;

namespace ToBot.App
{
    public class Config
    {
        public static Config CurrentConfig { get; set; }

        public Config()
        {
            #region App

            LogsDirectory = "";
            LogFilePath = "";
            BackupDirectory = "";
            DatabaseFilePath = "db.litedb";

            #endregion

            #region DiscordConfiguration

            Token = null;
            TokenType = TokenType.Bot;
            UseInternalLogHandler = true;
            LogLevel = LogLevel.Debug;
            CommandPrefix = "!";
            AutoReconnect = false;

            #endregion

            NotificationTimeout = 36000000000L;
            StatisticsNotificationTimeout = 864000000000L;
        }

        #region App

        public string LogsDirectory { get; set; }

        public string LogFilePath { get; set; }

        public string DatabaseFilePath { get; set; }

        public string BackupDirectory { get; set; }

        #endregion

        #region DiscordConfiguration

        public string Token { get; set; }

        public TokenType TokenType { get; set; }

        public bool UseInternalLogHandler { get; set; }

        public LogLevel LogLevel { get; set; }

        public string CommandPrefix { get; set; }

        public bool AutoReconnect { get; set; }

        #endregion

        #region Common

        public long NotificationTimeout { get; set; }

        #endregion

        #region Statistics

        public long StatisticsNotificationTimeout { get; set; }

        #endregion

        #region ToDiscordConfiguration

        public DiscordConfiguration ToDiscordConfiguration()
        {
            return new DiscordConfiguration
            {
                Token = Token,
                TokenType = TokenType,
                UseInternalLogHandler = UseInternalLogHandler,
                LogLevel = LogLevel,
                AutoReconnect = AutoReconnect
            };
        }

        #endregion
    }
}
