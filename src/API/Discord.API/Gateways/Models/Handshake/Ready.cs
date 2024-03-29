﻿// Quarrel © 2022

using Discord.API.Models.Json.Channels;
using Discord.API.Models.Json.Guilds;
using Discord.API.Models.Json.Settings;
using Discord.API.Models.Json.Users;
using System.Text.Json.Serialization;

// JSON models don't need to respect standard nullable rules.
#pragma warning disable CS8618

namespace Discord.API.Gateways.Models.Handshake
{
    internal record Ready
    {
        [JsonPropertyName("v")]
        public int GatewayVersion { get; set; }

        [JsonPropertyName("user_settings")]
        public JsonUserSettings Settings { get; set; }

        [JsonPropertyName("user_guild_settings")]
        public JsonGuildSettings[] GuildSettings { get; set; }

        [JsonPropertyName("user")]
        public JsonUser User { get; set; }

        [JsonPropertyName("guilds")]
        public JsonGuild[] Guilds { get; set; }

        [JsonPropertyName("private_channels")]
        public JsonChannel[] PrivateChannels { get; set; }

        [JsonPropertyName("session_id")]
        public string SessionId { get; set; }

        [JsonPropertyName("presences")]
        public JsonPresence[] Presences { get; set; }

        [JsonPropertyName("relationships")]
        public JsonRelationship[] Relationships { get; set; }

        [JsonPropertyName("read_state")]
        public JsonReadState[] ReadStates { get; set; }
    }
}
