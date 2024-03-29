﻿// Quarrel © 2022

using System.Text.Json.Serialization;

// JSON models don't need to respect standard nullable rules.
#pragma warning disable CS8618

namespace Discord.API.Gateways.Models.GuildMember
{
    internal class GuildMemberListUpdated
    {
        [JsonPropertyName("online_count")]
        public int OnlineCount { get; set; }

        [JsonPropertyName("member_count")]
        public ulong MemberCount { get; set; }

        [JsonPropertyName("guild_id"), JsonNumberHandling(Constants.ReadWriteAsString)]
        public ulong GuildId { get; set; }

        [JsonPropertyName("groups")]
        public Group[] Groups { get; set; }
    }
}
