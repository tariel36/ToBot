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

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using ToBot.Common.Extensions;
using ToBot.Common.Maintenance.Logging;
using ToBot.Common.Pocos;
using ToBot.Communication.Commands;
using ToBot.Communication.Messaging;
using ToBot.Discord;
using ToBot.Plugin.Plugins;
using ToBot.Common.Threading;

namespace ToBot.Plugins
{
    public class PluginsManager
    {
        public PluginsManager(ILogger logger)
        {
            Logger = logger;

            Plugins = new Dictionary<Type, RegisteredPlugin>();
        }

        public ILogger Logger { get; set; }

        private Dictionary<Type, RegisteredPlugin> Plugins { get; }

        public async Task OnMessage(Type associatedPluginType, string command, CommandContext ctx, CommandArguments args)
        {
            if (Plugins.ContainsKey(associatedPluginType))
            {
                RegisteredCommand registeredCommand = Plugins[associatedPluginType].GetCommand(command);

                if (registeredCommand != null)
                {
                    await registeredCommand.Execute(new CommandExecutionContext(command,
                        new CommunicationContext(ctx.Message.Id,
                            ctx.Channel.Id,
                            ctx.Channel.Name,
                            ctx.User.Id,
                            ctx.User.Username,
                            ctx.Channel.Type == DSharpPlus.ChannelType.Private,
                            msg => ctx.RespondAsync(msg),
                            sb => ctx.RespondAsync(sb.ToString()),
                            (ulong channelId, string msg) => WriteToChannel(ctx, channelId, msg),
                            (ulong userId, ulong guildId, string roleName) => SetUserRole(ctx, userId, guildId, roleName),
                            (ulong userId, ulong guildId, string roleName) => RemoveUserRole(ctx, userId, guildId, roleName),
                            (ulong userId, ulong guildId, TimeSpan timeout, string keyword, string timeoutMessage) => WaitForUserReply(ctx, userId, guildId, timeout, keyword, timeoutMessage),
                            (ulong userId, ulong guildId, string roleName) => UserHasRole(ctx, userId, guildId, roleName),
                            (ulong userId, ulong guildId, string name) => SetUserName(ctx, userId, guildId, name),
                            (ulong guildId) => GetGuildMembers(ctx, guildId),
                            (ulong guildId, string roleName) => RoleExists(ctx, guildId, roleName),
                            (ulong guildId, ulong userId) => CreateChannel(ctx, guildId, userId),
                            (ulong toChannelId, ulong[] messageId) => MoveMessage(ctx, toChannelId, messageId),
                            (ulong channelId, int count) => GetTopMessageId(ctx, channelId, count),
                            (ulong channelId, ulong messageId) => DeleteMessage(ctx, channelId, messageId)
                            ),
                        args));
                }
            }
            else
            {
                await ctx.RespondAsync($"Invalid plugin: {associatedPluginType.Name}");
            }
        }

        public void RegisterPlugin(BasePlugin plugin,
            Type commandProxyType,
            Action<Type> commandsRegister,
            BasePlugin.NotificationDelegate pluginOnNotification)
        {
            Type type = plugin.GetType();

            if (!Plugins.ContainsKey(type))
            {
                string commandsPrefix = type.GetField("Prefix", BindingFlags.Public | BindingFlags.Static)?.GetValue(plugin)?.ToString();

                RegisteredPlugin registered = new RegisteredPlugin(plugin, commandsPrefix, CreateCommands(plugin, commandProxyType));

                string[] commandsName = registered.GetCommands().Select(x => x.Name).ToArray();

                foreach (RegisteredPlugin registeredPlugin in Plugins.Values)
                {
                    string[] registeredCommands = registeredPlugin.GetCommands().Select(x => x.Name).ToArray();

                    if (commandsName.Intersect(registeredCommands).Any())
                    {
                        throw new InvalidOperationException($"Can't register plugin {plugin.Name} because it intersects commands with {registeredPlugin.Name}");
                    }
                }

                Plugins[type] = registered;

                if (pluginOnNotification != null)
                {
                    registered.Plugin.NotificationEvent.Event += new EventHandler<NotificationContext>(pluginOnNotification); 
                }

                commandsRegister(commandProxyType);

                registered.Plugin.Start();
            }
        }

        public void UnregisterPlugin(BasePlugin plugin)
        {
            Type type = plugin.GetType();

            if (Plugins.ContainsKey(type))
            {
                Plugins[type].Plugin.Stop();
                Plugins[type].SafeDispose();
                Plugins.Remove(type);
            }
        }

        public void UnregisterAll()
        {
            BasePlugin[] plugins = Plugins.Values.Select(x => x.Plugin).ToArray();
            foreach (BasePlugin plugin in plugins)
            {
                string pluginName = plugin.Name;

                try
                {
                    UnregisterPlugin(plugin);
                    Logger.LogMessage(LogLevel.Debug, nameof(PluginsManager), $"Unregistered {pluginName}.");
                }
                catch (Exception ex)
                {
                    Logger.LogMessage(LogLevel.Error, nameof(PluginsManager), $"Failed to unregister {pluginName}. {ex}");
                }
            }
        }

        public async Task GuildMemberAdded(DSharpPlus.DiscordClient client, ulong id)
        {
            DSharpPlus.Entities.DiscordDmChannel dm = await client.CreateDmAsync(await client.GetUserAsync(id));

            foreach (RegisteredPlugin plugin in Plugins.Values)
            {
                await plugin.Plugin.GuildMemberAdded(id, m => dm.SendMessageAsync(m));
            }
        }

        private Dictionary<string, RegisteredCommand> CreateCommands(BasePlugin plugin, Type commandProxyType)
        {
            Dictionary<string, RegisteredCommand> registeredCommands = new Dictionary<string, RegisteredCommand>();

            foreach (MethodInfo cmdInfo in commandProxyType
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.GetCustomAttribute<CommandAttribute>() != null))
            {
                string commandName = cmdInfo.GetCustomAttribute<CommandAttribute>().Name;
                string methodName = cmdInfo.Name;

                MethodInfo mi = commandProxyType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (mi == null)
                {
                    Logger.LogMessage(LogLevel.Debug, plugin.Name, $"Method {methodName} not found.");
                }
                else
                {
                    string descr = cmdInfo.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
                    string[] aliases = cmdInfo.GetCustomAttribute<AliasesAttribute>()?.Aliases?.ToArray() ?? new string[0];

                    List<RegisteredCommand.CommandParameter> args = new List<RegisteredCommand.CommandParameter>();
                    foreach (ParameterInfo pInfo in cmdInfo.GetParameters())
                    {
                        bool isRequired = pInfo.GetCustomAttribute<RequiredAttribute>() != null;
                        string pDescr = pInfo.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;

                        args.Add(new RegisteredCommand.CommandParameter(pInfo.Name, pDescr, isRequired));
                    }

                    MethodInfo methodInfo = plugin.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    CommandDelegate invocator = methodInfo?.CreateDelegate(typeof(CommandDelegate), plugin) as CommandDelegate;

                    try
                    {
                        registeredCommands[commandName] = new RegisteredCommand(invocator,
                            commandName,
                            methodName,
                            descr,
                            args,
                            aliases,
                            null,
                            0
                            );
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage(LogLevel.Error, plugin.Name, $"Failed to register command {commandName} ({methodName}). Exception: {ex}");
                    }
                }
            }

            return registeredCommands;
        }

        private async Task<bool> UserHasRole(CommandContext ctx, ulong userId, ulong guildId, string roleName)
        {
            DSharpPlus.Entities.DiscordGuild guild = await ctx?.Client?.GetGuildAsync(guildId);
            DSharpPlus.Entities.DiscordMember user = await guild?.GetMemberAsync(userId);

            return user?.Roles?.Any(x => string.Equals(x?.Name, roleName, StringComparison.InvariantCultureIgnoreCase)) == true;
        }

        private async Task<bool> WaitForUserReply(CommandContext ctx, ulong userId, ulong guildId, TimeSpan timeout, string keyword, string timeoutMessage)
        {
            InteractivityModule interactivity = ctx.Client.GetInteractivityModule();

            MessageContext reply = await interactivity.WaitForMessageAsync(x => x.Author.Id == userId && string.Equals(x.Content, keyword, StringComparison.InvariantCultureIgnoreCase), timeout);

            return reply != null;
        }

        private async Task SetUserRole(CommandContext ctx, ulong userId, ulong guildId, string roleName)
        {
            DSharpPlus.Entities.DiscordGuild guild = await ctx?.Client?.GetGuildAsync(guildId);
            DSharpPlus.Entities.DiscordMember user = await guild?.GetMemberAsync(userId);

            DSharpPlus.Entities.DiscordRole role = guild?.Roles?.FirstOrDefault(x => string.Equals(x.Name, roleName, StringComparison.InvariantCultureIgnoreCase));

            if (user != null && role != null)
            {
                await user.GrantRoleAsync(role); 
            }
        }

        private async Task RemoveUserRole(CommandContext ctx, ulong userId, ulong guildId, string roleName)
        {
            DSharpPlus.Entities.DiscordGuild guild = await ctx?.Client?.GetGuildAsync(guildId);
            DSharpPlus.Entities.DiscordMember user = await guild?.GetMemberAsync(userId);

            DSharpPlus.Entities.DiscordRole role = guild?.Roles?.FirstOrDefault(x => string.Equals(x.Name, roleName, StringComparison.InvariantCultureIgnoreCase));

            if (user != null && role != null)
            {
                await user.RevokeRoleAsync(role); 
            }
        }

        private async Task SetUserName(CommandContext ctx, ulong userId, ulong guildId, string name)
        {
            DSharpPlus.Entities.DiscordGuild guild = await ctx?.Client?.GetGuildAsync(guildId);
            DSharpPlus.Entities.DiscordMember user = await guild?.GetMemberAsync(userId);

            await user?.ModifyAsync(nickname: name);
        }

        private async Task WriteToChannel(CommandContext ctx, ulong channelId, string msg)
        {
            DSharpPlus.Entities.DiscordChannel channel = await ctx?.Client?.GetChannelAsync(channelId);

            await channel?.SendMessageAsync(msg);
        }

        private async Task<ICollection<GuildMember>> GetGuildMembers(CommandContext ctx, ulong guildId)
        {
            List<GuildMember> members = null;

            DSharpPlus.Entities.DiscordGuild guild = await ctx?.Client?.GetGuildAsync(guildId);

            members = guild?.Members?.Select(x => new GuildMember()
            {
                Id = x.Id,
                Discriminator = x.Discriminator,
                UserName = x.Username,
                DisplayName = x.DisplayName,
                Nickname = x.Nickname,
                Roles = x.Roles?.Select(y => y.Name).ToArray() ?? new string[0]
            }).ToList();

            return members;
        }

        private async Task<bool> RoleExists(CommandContext ctx, ulong guildId, string roleName)
        {
            DSharpPlus.Entities.DiscordGuild guild = await ctx?.Client?.GetGuildAsync(guildId);

            return guild?.Roles?.Any(x => string.Equals(x.Name, roleName, StringComparison.InvariantCultureIgnoreCase)) == true;
        }

        private async Task<ulong> CreateChannel(CommandContext ctx, ulong guildId, ulong userId)
        {
            DSharpPlus.DiscordClient client = ctx?.Client;
            DSharpPlus.Entities.DiscordGuild guild = await client?.GetGuildAsync(guildId);
            DSharpPlus.Entities.DiscordMember user = await guild?.GetMemberAsync(userId);
            DSharpPlus.Entities.DiscordDmChannel dm = await client?.CreateDmAsync(user);

            return dm?.Id ?? 0;
        }

        private async Task<bool> MoveMessage(CommandContext ctx, ulong toChannelId, ulong[] messagesId)
        {
            if (ctx?.Client == null)
            {
                return false;
            }

            DSharpPlus.DiscordClient client = ctx.Client;
            DSharpPlus.Entities.DiscordChannel targetChannel = await client.GetChannelAsync(toChannelId);
            DSharpPlus.Entities.DiscordChannel sourceChannel = ctx.Channel;

            foreach (ulong messageId in messagesId)
            {
                DiscordMessage message = await sourceChannel.GetMessageAsync(messageId);
                string header = $"{message.Author.Username} on {message.Channel.Name} at {message.CreationTimestamp:yyyy-MM-dd HH:mm:ss}";
                string content = message.Content;
                string[] attachments = message.Attachments.Select(x => x.Url).ToArray();
                string separator = "\n";
                string allAttachments = string.Join(separator, attachments);
                string newMessage = string.Join(separator, header, content);

                if (newMessage.Length >= 2000)
                {
                    await ctx.Client.SendMessageAsync(targetChannel, header);
                    await ctx.Client.SendMessageAsync(targetChannel, content);
                }
                else
                {
                    await ctx.Client.SendMessageAsync(targetChannel, newMessage);
                }

                if (!string.IsNullOrWhiteSpace(allAttachments) && allAttachments.Length > 0)
                {
                    await Tasks.Throttle();

                    if (allAttachments.Length >= 2000)
                    {
                        foreach (string msg in new DiscordMessageFormatter().SplitMessage(allAttachments))
                        {
                            await ctx.Client.SendMessageAsync(targetChannel, msg);
                            await Tasks.Throttle();
                        }
                    }
                    else
                    {
                        await ctx.Client.SendMessageAsync(targetChannel, allAttachments);
                    } 
                }
            }

            return true;
        }

        private async Task DeleteMessage(CommandContext ctx, ulong channelId, ulong messageId)
        {
            if (ctx?.Client == null)
            {
                return;
            }

            DiscordChannel channel = await ctx.Client.GetChannelAsync(channelId);
            DiscordMessage message = await channel.GetMessageAsync(messageId);

            await channel.DeleteMessageAsync(message);
        }

        private async Task<ulong[]> GetTopMessageId(CommandContext ctx, ulong channelId, int count)
        {
            if (ctx?.Client == null)
            {
                return new ulong[0];
            }

            DiscordChannel channel = await ctx.Client.GetChannelAsync(channelId);
            IReadOnlyList<DiscordMessage> messages = await channel.GetMessagesAsync(limit: count);

            return messages.Select(x => x.Id).ToArray();
        }
    }
}
