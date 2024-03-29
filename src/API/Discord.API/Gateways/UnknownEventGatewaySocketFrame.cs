﻿// Quarrel © 2022

using System.Text.Json.Serialization;

// JSON models don't need to respect standard nullable rules.
#pragma warning disable CS8618

namespace Discord.API.Gateways
{
    internal class UnknownEventGatewaySocketFrame : GatewaySocketFrame
    {
        [JsonPropertyName("t")]
        public new string Event { get; set; }
    }
}
