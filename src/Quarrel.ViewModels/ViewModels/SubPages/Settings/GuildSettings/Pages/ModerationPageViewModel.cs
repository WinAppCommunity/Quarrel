﻿// Quarrel © 2022

using Discord.API.Models.Enums.Guilds;
using Discord.API.Models.Enums.Settings;
using Microsoft.Toolkit.Mvvm.Input;
using Quarrel.Bindables.Guilds;
using Quarrel.Services.Discord;
using Quarrel.Services.Localization;
using Quarrel.ViewModels.SubPages.Settings.Abstract;
using Quarrel.ViewModels.SubPages.Settings.GuildSettings.Pages.Abstract;

namespace Quarrel.ViewModels.SubPages.Settings.GuildSettings.Pages
{
    /// <summary>
    /// A view model for the moderation page in guild settings.
    /// </summary>
    public class ModerationPageViewModel : GuildSettingsSubPageViewModel
    {
        private const string ModerationResource = "GuildSettings/Moderation";

        private DraftValue<VerificationLevel> _verificationLevel;
        private DraftValue<ExplicitContentFilterLevel> _explicitContentFilterLevel;

        internal ModerationPageViewModel(ILocalizationService localizationService, IDiscordService discordService, BindableGuild guild) :
            base(localizationService, discordService, guild)
        {
            _verificationLevel = new(guild.Guild.VerificationLevel);
            _explicitContentFilterLevel = new(guild.Guild.ExplicitContentFilter);

            SetVerificationLevelCommand = new RelayCommand<VerificationLevel>(SetVerificationLevel);
            SetExplicitContentFilterLevelCommand = new RelayCommand<ExplicitContentFilterLevel>(SetExplicitContentFilterLevel);

            RegisterDraftValues(VerificationLevel, ExplicitContentFilterLevel);
        }

        /// <inheritdoc/>
        public override string Glyph => "";

        /// <inheritdoc/>
        public override string Title => _localizationService[ModerationResource];

        /// <inheritdoc/>
        public override bool IsActive => true;

        /// <summary>
        /// Gets or sets the verification level.
        /// </summary>
        public DraftValue<VerificationLevel> VerificationLevel
        {
            get => _verificationLevel;
            set => SetProperty(ref _verificationLevel, value);
        }

        /// <summary>
        /// Gets or sets the explicit content filter level.
        /// </summary>
        public DraftValue<ExplicitContentFilterLevel> ExplicitContentFilterLevel
        {
            get => _explicitContentFilterLevel;
            set => SetProperty(ref _explicitContentFilterLevel, value);
        }

        /// <summary>
        /// Gets a command that sets the verification level.
        /// </summary>
        public RelayCommand<VerificationLevel> SetVerificationLevelCommand { get; }

        /// <summary>
        /// Gets a command that sets the verification level.
        /// </summary>
        public RelayCommand<ExplicitContentFilterLevel> SetExplicitContentFilterLevelCommand { get; }

        private void SetVerificationLevel(VerificationLevel verificationLevel)
            => VerificationLevel.Value = verificationLevel;

        private void SetExplicitContentFilterLevel(ExplicitContentFilterLevel explicitContentFilterLevel)
            => ExplicitContentFilterLevel.Value = explicitContentFilterLevel;

        /// <inheritdoc/>
        protected override void ApplyChanges()
        {
        }
    }
}
  