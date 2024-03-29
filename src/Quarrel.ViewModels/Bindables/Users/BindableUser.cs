﻿// Quarrel © 2022

using Microsoft.Toolkit.Mvvm.Messaging;
using Quarrel.Bindables.Abstract;
using Quarrel.Bindables.Users.Interfaces;
using Quarrel.Client;
using Quarrel.Client.Models.Users;
using Quarrel.Services.Discord;
using Quarrel.Services.Dispatcher;
using System;

namespace Quarrel.Bindables.Users
{
    /// <summary>
    /// A wrapper of a <see cref="Client.Models.Users.User"/> that can be bound to the UI.
    /// </summary>
    public partial class BindableUser : BindableItem, IBindableUser
    {
        private User _user;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BindableUser"/> class.
        /// </summary>
        internal BindableUser(
            IMessenger messenger,
            IDiscordService discordService,
            QuarrelClient quarrelClient,
            IDispatcherService dispatcherService,
            User user) :
            base(messenger, discordService, quarrelClient, dispatcherService)
        {
            _user = user;
        }
        
        /// <inheritdoc/>
        public User User
        {
            get => _user;
            set
            {
                if (SetProperty(ref _user, value))
                {
                    OnPropertyChanged(nameof(AvatarUrl));
                    OnPropertyChanged(nameof(AvatarUri));
                }
            }
        }
        
        /// <inheritdoc/>
        public string? AvatarUrl => User.GetAvatarUrl();
        
        /// <inheritdoc/>
        public Uri? AvatarUri => AvatarUrl is null ? null : new(AvatarUrl);
    }
}
