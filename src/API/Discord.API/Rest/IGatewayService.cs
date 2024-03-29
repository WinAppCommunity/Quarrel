﻿// Quarrel © 2022

using Discord.API.Models.Json.Gateway;
using Refit;
using System.Threading.Tasks;

namespace Discord.API.Rest
{
    internal interface IGatewayService
    {
        [Get("/gateway")]
        Task<GatewayConfig> GetGatewayConfig();
    }
}
