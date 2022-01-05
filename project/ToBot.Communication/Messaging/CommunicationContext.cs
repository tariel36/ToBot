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

namespace ToBot.Communication.Messaging
{
    public class CommunicationContext
    {
        public CommunicationContext(ulong messageId,
            ulong channelId,
            string channelName,
            ulong userId,
            string userName,
            bool isDirectMessage,
            RespondStringAsyncDelegate respondStringAsync,
            RespondStringBuilderAsyncDelegate respondStringBuilderAsync,
            WriteToChannelAsyncDelegate writeToChannelAsync,
            SetRoleAsyncDelegate setRole,
            RemoveRoleAsyncDelegate removeRole,
            WaitForUserReplyAsyncDelegate waitForUserReply,
            UserHasRoleAsyncDelegate userHasRole,
            SetNameAsyncDelegate setUserName,
            GetGuildMembersAsyncDelegate getGuildMembers,
            RoleExistsAsyncDelegate roleExists,
            CreateChannelAsyncDelegate createChannel,
            MoveMessageAsyncDelegate moveMessage,
            GetTopMessageIdAsync getTopMessageId,
            DeleteMessageAsync deleteMessage
            )
        {
            MessageId = messageId;
            ChannelId = channelId;
            ChannelName = channelName;
            UserId = userId;

            UserName = userName;

            IsDirectMessage = isDirectMessage;

            RespondStringAsync = respondStringAsync;
            RespondStringBuilderAsync = respondStringBuilderAsync;

            WriteToChannelAsync = writeToChannelAsync;

            SetUserName = setUserName;
            SetRole = setRole;
            RemoveRole = removeRole;
            WaitForUserReply = waitForUserReply;
            UserHasRole = userHasRole;

            GetGuildMembers = getGuildMembers;
            RoleExists = roleExists;
            CreateChannel = createChannel;
            MoveMessage = moveMessage;

            GetTopMessageId = getTopMessageId;
            DeleteMessage = deleteMessage;
        }

        public ulong MessageId { get; }

        public ulong ChannelId { get; }

        public string ChannelName { get; }

        public ulong UserId { get; }

        public string UserName { get; }

        public bool IsDirectMessage { get; }

        public RespondStringAsyncDelegate RespondStringAsync { get; }

        public RespondStringBuilderAsyncDelegate RespondStringBuilderAsync { get; }

        public WriteToChannelAsyncDelegate WriteToChannelAsync { get; }

        public SetNameAsyncDelegate SetUserName { get; }

        public SetRoleAsyncDelegate SetRole { get; }

        public RemoveRoleAsyncDelegate RemoveRole { get; }

        public WaitForUserReplyAsyncDelegate WaitForUserReply { get; }

        public UserHasRoleAsyncDelegate UserHasRole { get; }

        public GetGuildMembersAsyncDelegate GetGuildMembers { get; }

        public RoleExistsAsyncDelegate RoleExists { get; }

        public CreateChannelAsyncDelegate CreateChannel { get; }

        public MoveMessageAsyncDelegate MoveMessage { get; }

        public GetTopMessageIdAsync GetTopMessageId { get; }

        public DeleteMessageAsync DeleteMessage { get; }
    }
}
