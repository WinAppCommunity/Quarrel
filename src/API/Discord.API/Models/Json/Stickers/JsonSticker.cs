﻿// Quarrel © 2022

using Discord.API.Models.Json.Users;
using Discord.API.Models.Enums.Stickers;
using System.Text.Json.Serialization;

// JSON models don't need to respect standard nullable rules.
#pragma warning disable CS8618

namespace Discord.API.Models.Json.Stickers
{
    internal record JsonSticker
    {
        [JsonPropertyName("id"), JsonNumberHandling(Constants.ReadWriteAsString)]
        public ulong Id { get; set; }

        [JsonPropertyName("pack_id")]
        public ulong PackId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("tags")]
        public string? Tags { get; set; }

        [JsonPropertyName("type")]
        public StickerType Type { get; set; }

        [JsonPropertyName("format_type")]
        public StickerFormatType FormatType { get; set; }

        [JsonPropertyName("available")]
        public bool? Available { get; set; }

        [JsonPropertyName("guild_id"), JsonNumberHandling(Constants.ReadWriteAsString)]
        public ulong? GuildId { get; set; }

        [JsonPropertyName("user")]
        public JsonUser? User { get; set; }

        [JsonPropertyName("sort_value")]
        public int? SortValue { get; set; }
    }
}
