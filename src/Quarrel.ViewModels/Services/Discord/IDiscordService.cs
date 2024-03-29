﻿// Quarrel © 2022

using Discord.API.Models.Enums.Users;
using Quarrel.Bindables.Channels.Abstract;
using Quarrel.Bindables.Channels.Interfaces;
using Quarrel.Bindables.Guilds;
using Quarrel.Bindables.Messages;
using Quarrel.Bindables.Users;
using Quarrel.Client.Models.Guilds;
using Quarrel.Client.Models.Messages;
using Quarrel.Client.Models.Settings;
using Quarrel.Client.Models.Users;
using Quarrel.Services.Analytics.Enums;
using System.Threading.Tasks;

namespace Quarrel.Services.Discord
{
    /// <summary>
    /// An interface for a service that handles interactions with the discord client.
    /// </summary>
    public interface IDiscordService
    {
        /// <summary>
        /// Gets the id of the current user.
        /// </summary>
        ulong? MyId { get; }
        
        /// <summary>
        /// Modifies the current user.
        /// </summary>
        /// <param name="modifyUser">The user modifications.</param>
        Task ModifyMe(ModifySelfUser modifyUser);

        /// <summary>
        /// Gets the current discord settings.
        /// </summary>
        UserSettings? GetSettings();

        /// <summary>
        /// Modifies user settings.
        /// </summary>
        /// <param name="modifySettings">The settings modifications.</param>
        Task ModifySettings(ModifyUserSettings modifySettings);
        
        /// <summary>
        /// Logs into the discord service by token.
        /// </summary>
        /// <param name="token">The token to use for login.</param>
        /// <param name="source">The login source.</param>
        Task<bool> LoginAsync(string token, LoginType source = LoginType.Unspecified);
        
        /// <summary>
        /// Modifies a guild.
        /// </summary>
        /// <param name="id">The id of the guild to modify.</param>
        /// <param name="modifyGuild">The guild modifications.</param>
        Task ModifyGuild(ulong id, ModifyGuild modifyGuild);

        /// <summary>
        /// Gets the messages in a channel.
        /// </summary>
        /// <param name="channel">The channel to get the messages for.</param>
        /// <param name="beforeId">The if of the last message to load messages before, or null.</param>
        /// <returns>An array of <see cref="BindableMessage"/>s from the channel.</returns>
        Task<Message[]> GetChannelMessagesAsync(IBindableMessageChannel channel, ulong? beforeId = null);
        
        /// <summary>
        /// Gets the user's direct message channels.
        /// </summary>
        /// <param name="home">The <see cref="BindableHomeItem"/>.</param>
        /// <param name="selectedChannel">The selected channel as an <see cref="IBindableSelectableChannel"/>.</param>
        /// <returns>An array of <see cref="BindablePrivateChannel"/>s.</returns>
        BindablePrivateChannel?[] GetPrivateChannels(BindableHomeItem home, out IBindableSelectableChannel? selectedChannel);
        
        /// <summary>
        /// Marks a messages as the last read message in a channel.
        /// </summary>
        /// <param name="channelId">The id of the channel.</param>
        /// <param name="messageId">The id of the message.</param>
        Task MarkRead(ulong channelId, ulong messageId);

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="channelId">The id of the channel to send the message in.</param>
        /// <param name="content">The content of the message.</param>
        Task SendMessage(ulong channelId, string content);

        /// <summary>
        /// Deletes a message.
        /// </summary>
        /// <param name="channelId">The id of the channel the message is in.</param>
        /// <param name="messageId">The id of the message to delete.</param>
        Task DeleteMessage(ulong channelId, ulong messageId);

        /// <summary>
        /// Starts a call in a private channel.
        /// </summary>
        /// <param name="channelId">The id of the private channel.</param>
        Task StartCall(ulong channelId);

        /// <summary>
        /// Joins a voice channel.
        /// </summary>
        /// <param name="channelId">The id of the voice or private channel.</param>
        /// <param name="guildId">The guild if of the voice channel</param>
        Task JoinCall(ulong channelId, ulong? guildId = null);

        /// <summary>
        /// Leaves any current call.
        /// </summary>
        Task LeaveCall();

        /// <summary>
        /// Joins a user's stream.
        /// </summary>
        /// <param name="userId">The id of the user who's stream to join.</param>
        Task JoinStream(ulong userId);

        /// <summary>
        /// Updates the user's online status.
        /// </summary>
        /// <param name="status">The new online status to set.</param>
        Task SetStatus(UserStatus status);
    }
}
