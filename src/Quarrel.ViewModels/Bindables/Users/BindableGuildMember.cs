﻿// Quarrel © 2022

using Microsoft.Toolkit.Mvvm.Messaging;
using Quarrel.Bindables.Abstract;
using Quarrel.Client;
using Quarrel.Client.Models.Users;
using Quarrel.Services.Discord;
using Quarrel.Services.Dispatcher;

namespace Quarrel.Bindables.Users
{
    /// <summary>
    /// A wrapper of a <see cref="Client.Models.Users.GuildMember"/> that can be bound to the UI.
    /// </summary>
    public class BindableGuildMember : BindableItem
    {
        private GuildMember _guildMember;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindableGuildMember"/> class.
        /// </summary>
        internal BindableGuildMember(
            IMessenger messenger,
            IDiscordService discordService,
            QuarrelClient quarrelClient,
            IDispatcherService dispatcherService,
            GuildMember guildMember) :
            base(messenger, discordService, quarrelClient, dispatcherService)
        {
            _guildMember = guildMember;
        }

        /// <summary>
        /// Gets the wrapped <see cref="Client.Models.Users.GuildMember"/>.
        /// </summary>
        public GuildMember GuildMember
        {
            get => _guildMember;
            set => SetProperty(ref _guildMember, value);
        }
    }
}
