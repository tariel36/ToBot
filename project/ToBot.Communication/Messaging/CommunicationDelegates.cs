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
using System.Text;
using System.Threading.Tasks;
using ToBot.Common.Pocos;
using ToBot.Communication.Commands;

namespace ToBot.Communication.Messaging
{
    public delegate Task RespondStringBuilderAsyncDelegate(StringBuilder stringBuilder);

    public delegate Task RespondStringAsyncDelegate(string message);

    public delegate Task CommandDelegate(CommandExecutionContext ctx);

    public delegate Task SetNameAsyncDelegate(ulong userId, ulong guildId, string name);

    public delegate Task SetRoleAsyncDelegate(ulong userId, ulong guildId, string role);

    public delegate Task RemoveRoleAsyncDelegate(ulong userId, ulong guildId, string role);

    public delegate Task<bool> WaitForUserReplyAsyncDelegate(ulong userId, ulong guildId, TimeSpan timeout, string keyword, string timeoutMessage);

    public delegate Task<bool> UserHasRoleAsyncDelegate(ulong userId, ulong guildId, string roleName);

    public delegate Task WriteToChannelAsyncDelegate(ulong channelId, string msg);

    public delegate Task<ICollection<GuildMember>> GetGuildMembersAsyncDelegate(ulong guildId);

    public delegate Task<bool> RoleExistsAsyncDelegate(ulong guildId, string name);

    public delegate Task<ulong> CreateChannelAsyncDelegate(ulong guildId, ulong userId);

    public delegate Task<bool> MoveMessageAsyncDelegate(ulong toId, params ulong[] toMove);

    public delegate Task<ulong[]> GetTopMessageIdAsync(ulong channelId, int count);

    public delegate Task DeleteMessageAsync(ulong channelId, ulong messageId);
}
