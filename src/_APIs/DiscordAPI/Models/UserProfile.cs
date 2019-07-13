﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordAPI.Models
{
    public class UserProfile
    {
        [JsonProperty("premium_since")]
        public DateTime? PremiumSince { get; set; }

        [JsonProperty("mutual_guilds")]
        public IEnumerable<MutualGuild> MutualGuilds { get; set; }

        public User user { get; set; }

        [JsonProperty("connected_accounts")]
        public IEnumerable<ConnectedAccount> ConnectedAccounts { get; set; }

        [JsonIgnore]
        public IEnumerable<SharedFriend> SharedFriends { get; set; }

        [JsonIgnore]
        public Friend Friend { get; set; }
    }
}
