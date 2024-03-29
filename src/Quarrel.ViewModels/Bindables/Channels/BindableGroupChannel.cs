﻿// Quarrel © 2022

using CommunityToolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.Messaging;
using Quarrel.Bindables.Channels.Abstract;
using Quarrel.Bindables.Users;
using Quarrel.Client;
using Quarrel.Client.Models.Channels;
using Quarrel.Client.Models.Channels.Interfaces;
using Quarrel.Services.Clipboard;
using Quarrel.Services.Discord;
using Quarrel.Services.Dispatcher;
using Quarrel.Services.Localization;
using System.Linq;

namespace Quarrel.Bindables.Channels
{
    /// <summary>
    /// A wrapper of an <see cref="IGroupChannel"/> that can be bound to the UI.
    /// </summary>
    public class BindableGroupChannel : BindablePrivateChannel
    {
        private readonly ILocalizationService _localizationService;

        internal BindableGroupChannel(
            IMessenger messenger,
            IClipboardService clipboardService,
            IDiscordService discordService,
            QuarrelClient quarrelClient,
            ILocalizationService localizationService,
            IDispatcherService dispatcherService,
            GroupChannel groupChannel) :
            base(messenger, clipboardService, discordService, quarrelClient, dispatcherService, groupChannel)
        {
            _localizationService = localizationService;

            Guard.IsNotNull(groupChannel.Recipients);
            Recipients = new BindableUser[groupChannel.Recipients.Length];
            int i = 0;
            foreach (var recipient in groupChannel.Recipients)
            {
                var user = _quarrelClient.Users.GetUser(recipient.Id);
                Guard.IsNotNull(user);
                Recipients[i] = new BindableUser(_messenger, _discordService, _quarrelClient, _dispatcherService, user);
                i++;
            }
        }

        /// <summary>
        /// Gets the wrapped channel as a <see cref="IGroupChannel"/>.
        /// </summary>
        public GroupChannel GroupChannel => (GroupChannel)Channel;

        /// <inheritdoc/>
        public override string? Name => Channel.Name ?? _localizationService.CommaList(Recipients.Select(x => x.User.Username).ToArray());

        /// <summary>
        /// Gets the icon url of the group channel.
        /// </summary>
        public string? IconUrl => GroupChannel.Icon is null ? null : $"https://cdn.discordapp.com/channel-icons/{Channel.Id}/{GroupChannel.Icon}.png";

        /// <summary>
        /// Gets the recipients of the group channel as a <see cref="BindableUser"/> array.
        /// </summary>
        public BindableUser[] Recipients { get; }

        /// <summary>
        /// Gets the number of members in the group channel.
        /// </summary>
        public int MemberCount => Recipients.Length + 1;
    }
}
