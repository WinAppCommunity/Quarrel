﻿// Quarrel © 2022

using CommunityToolkit.Diagnostics;
using Discord.API.Models.Enums.Guilds;
using Discord.API.Models.Enums.Settings;
using Discord.API.Models.Json.Guilds;
using Discord.API.Models.Json.Settings;
using Quarrel.Client.Models.Base;
using Quarrel.Client.Models.Channels.Interfaces;
using Quarrel.Client.Models.Guilds.Interfaces;
using Quarrel.Client.Models.Roles;
using Quarrel.Client.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quarrel.Client.Models.Guilds
{
    /// <summary>
    /// A guild managed by a <see cref="QuarrelClient"/>.
    /// </summary>
    public class Guild : SnowflakeItem, IGuild
    {
        private readonly HashSet<ulong> _channelIds;
        private readonly Dictionary<ulong, Role> _roles;

        /// <summary>
        /// Initializes new instance of the <see cref="Guild"/> class.
        /// </summary>
        internal Guild(JsonGuild restGuild, GuildSettings? settings, QuarrelClient context)
            : base(context)
        {
            Id = restGuild.Id;
            Name = restGuild.Name;

            AFKChannelId = restGuild.AFKChannelId;
            AFKTimeout = restGuild.AFKTimeout;

            DefaultMessageNotifications = restGuild.DefaultMessageNotifications;
            MfaLevel = restGuild.MfaLevel;
            VerificationLevel = restGuild.VerificationLevel;
            ExplicitContentFilter = restGuild.ExplicitContentFilter;
            NsfwLevel = restGuild.NsfwLevel;

            OwnerId = restGuild.OwnerId;

            PremiumSubscriptionCount = restGuild.PremiumSubscriptionCount ?? 0;
            IconId = restGuild.Icon;
            SplashId = restGuild.Splash;
            DiscoverySplashId = restGuild.DiscoverySplash;

            WidgetChannelId = restGuild.WidgetChannelId;
            SystemChannelId = restGuild.SystemChannelId;
            RulesChannelId = restGuild.RulesChannelId;
            PublicUpdatesChannelId = restGuild.PublicUpdatesChannelId;
            VoiceRegionId = restGuild.Region;

            IsWidgetEnabled = restGuild.IsWidgetEnabled ?? false;
            Available = true;

            Muted = settings?.Muted ?? false;

            _channelIds = new HashSet<ulong>();
            _roles = new Dictionary<ulong, Role>();

            foreach (var role in restGuild.Roles)
            {
                _roles.Add(role.Id, new Role(role, context));
            }
        }

        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public ulong? AFKChannelId { get; private set; }

        /// <inheritdoc/>
        public AFKTimeout? AFKTimeout { get; private set; }

        /// <inheritdoc/>
        public DefaultMessageNotifications DefaultMessageNotifications { get; private set; }

        /// <inheritdoc/>
        public MfaLevel MfaLevel { get; private set; }

        /// <inheritdoc/>
        public VerificationLevel VerificationLevel { get; private set; }

        /// <inheritdoc/>
        public ExplicitContentFilterLevel ExplicitContentFilter { get; private set; }

        /// <inheritdoc/>
        public NsfwLevel NsfwLevel { get; private set; }

        /// <inheritdoc/>
        public int PremiumSubscriptionCount { get; private set; }

        /// <inheritdoc/>
        public string IconId { get; private set; }

        /// <inheritdoc/>
        public string SplashId { get; private set; }

        /// <inheritdoc/>
        public string DiscoverySplashId { get; private set; }

        /// <inheritdoc/>
        public bool Available { get; private set; }

        /// <inheritdoc/>
        public ulong? WidgetChannelId { get; private set; }

        /// <inheritdoc/>
        public ulong? SystemChannelId { get; private set; }

        /// <inheritdoc/>
        public ulong? RulesChannelId { get; private set; }

        /// <inheritdoc/>
        public ulong? PublicUpdatesChannelId { get; private set; }

        /// <inheritdoc/>
        public bool IsWidgetEnabled { get; private set; }

        /// <inheritdoc/>
        public ulong OwnerId { get; private set; }

        /// <inheritdoc/>
        public string VoiceRegionId { get; private set; }

        /// <inheritdoc/>
        public bool Muted { get; private set; }

        /// <summary>
        /// Gets the channels in a guild
        /// </summary>
        public IGuildChannel[] GetChannels()
        {
            IGuildChannel[] channels = new IGuildChannel[_channelIds.Count];
            int i = 0;
            foreach (var channel in _channelIds.Select(Context.Channels.GetChannel))
            {
                if (channel is IGuildChannel guildChannel)
                {
                    channels[i] = guildChannel;
                    i++;
                }
            }

            Array.Resize(ref channels, i);

            return channels;
        }

        internal Role? GetRole(ulong roleId)
            => _roles.TryGetValue(roleId, out var role) ? role : null;

        internal void UpdateFromRestGuild(JsonGuild restGuild)
        {
            Guard.IsEqualTo(Id, restGuild.Id, nameof(Id));

            Name = restGuild.Name;
            AFKChannelId = restGuild.AFKChannelId ?? AFKChannelId;
            AFKTimeout = restGuild.AFKTimeout ?? AFKTimeout;
            DefaultMessageNotifications = restGuild.DefaultMessageNotifications;
            MfaLevel = restGuild.MfaLevel;
            VerificationLevel = restGuild.VerificationLevel;
            ExplicitContentFilter = restGuild.ExplicitContentFilter;
            NsfwLevel = restGuild.NsfwLevel;
            PremiumSubscriptionCount = restGuild.PremiumSubscriptionCount ?? 0;
            IconId = restGuild.Icon;
            SplashId = restGuild.Splash;
            DiscoverySplashId = restGuild.Splash;
            WidgetChannelId = restGuild.WidgetChannelId;
            SystemChannelId = restGuild.SystemChannelId;
            RulesChannelId = restGuild.RulesChannelId;
            PublicUpdatesChannelId = restGuild.PublicUpdatesChannelId;
            IsWidgetEnabled = restGuild.IsWidgetEnabled ?? false;
            OwnerId = restGuild.OwnerId;
            VoiceRegionId = restGuild.Region;

            Available = true;
        }

        internal bool AddChannel(ulong channelId)
            => _channelIds.Add(channelId);

        internal bool ContainsChannel(ulong channelId)
            => _channelIds.Contains(channelId);

        internal bool RemoveChannel(ulong channelId)
            => _channelIds.Remove(channelId);

        internal IEnumerable<ulong> ChannelIds => _channelIds;
    }
}
