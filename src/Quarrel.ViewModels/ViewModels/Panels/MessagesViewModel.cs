﻿// Adam Dernis © 2022

using CommunityToolkit.Diagnostics;
using Discord.API.Models.Channels.Interfaces;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Quarrel.Bindables.Channels.Interfaces;
using Quarrel.Bindables.Messages;
using Quarrel.Messages.Navigation;
using Quarrel.Services.Discord;
using Quarrel.Services.Dispatcher;
using System.Collections.ObjectModel;

namespace Quarrel.ViewModels.Panels
{
    /// <summary>
    /// The view model for the message list in the app.
    /// </summary>
    public partial class MessagesViewModel : ObservableRecipient
    {
        private readonly IMessenger _messenger;
        private readonly IDiscordService _discordService;
        private readonly IDispatcherService _dispatcherService;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagesViewModel"/> class.
        /// </summary>
        public MessagesViewModel(IMessenger messenger, IDiscordService discordService, IDispatcherService dispatcherService)
        {
            _messenger = messenger;
            _discordService = discordService;
            _dispatcherService = dispatcherService;

            Source = new ObservableRangeCollection<BindableMessage>();

            _messenger.Register<NavigateToChannelMessage>(this, (_, m) => LoadChannel(m.Channel));
        }

        /// <summary>
        /// The collection of loaded messages.
        /// </summary>
        public ObservableRangeCollection<BindableMessage> Source;

        private void LoadChannel(IBindableSelectableChannel channel)
        {
            if (channel is IBindableMessageChannel messageChannel)
            {
                LoadInitialMessages(messageChannel.MessageChannel);
            }
        }

        private async void LoadInitialMessages(IMessageChannel? channel)
        {
            Guard.IsNotNull(channel, nameof(channel));
            var messages = await _discordService.GetChannelMessagesAsync(channel);
            _dispatcherService.RunOnUIThread(() =>
            {
                Source.Clear();
                Source.AddRange(messages);
            });
        }
    }
}
