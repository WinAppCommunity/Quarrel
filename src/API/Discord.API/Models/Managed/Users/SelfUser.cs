﻿// Adam Dernis © 2022

using CommunityToolkit.Diagnostics;
using Discord.API.Models.Enums.Users;
using Discord.API.Models.Json.Users;
using Discord.API.Models.Users.Interfaces;

namespace Discord.API.Models.Users
{
    public class SelfUser : User, ISelfUser
    {
        internal SelfUser(JsonUser restUser, DiscordClient context) :
            base(restUser, context)
        {
            Guard.IsNotNull(restUser.PurchasedFlags, nameof(restUser.PurchasedFlags));

            Email = restUser.Email;
            Phone = restUser.Phone;
            Verified = restUser.Verified;
            MfaEnabled = restUser.MfaEnabled;
            NSFWAllowed = restUser.NSFWAllowed;
            PurchasedFlags = restUser.PurchasedFlags.Value;
            Locale = restUser.Locale;
        }

        public string? Email { get; private set; }

        public string? Phone { get; private set; }

        public bool? Verified { get; private set; }

        public bool? MfaEnabled { get; private set; }

        public bool? NSFWAllowed { get; private set; }

        public PremiumType PurchasedFlags { get; private set; }

        public string? Locale { get; private set; }

        internal override void UpdateFromRestUser(JsonUser jsonUser)
        {
            base.UpdateFromRestUser(jsonUser);

            Email = jsonUser.Email ?? Email;
            Phone = jsonUser.Phone ?? Phone;
            Verified = jsonUser.Verified ?? Verified;
            MfaEnabled = jsonUser.MfaEnabled ?? MfaEnabled;
            NSFWAllowed = jsonUser.NSFWAllowed ?? NSFWAllowed;
            PurchasedFlags = jsonUser.PurchasedFlags ?? PurchasedFlags;
            Locale = jsonUser.Locale ?? Locale;
        }

        internal override JsonUser ToRestUser()
        {
            JsonUser restUser = base.ToRestUser();
            restUser.Email = Email;
            restUser.Phone = Phone;
            restUser.Verified = Verified;
            restUser.MfaEnabled = MfaEnabled;
            restUser.NSFWAllowed = NSFWAllowed;
            restUser.PurchasedFlags = PurchasedFlags;
            restUser.Locale = Locale;
            return restUser;
        }
    }
}
