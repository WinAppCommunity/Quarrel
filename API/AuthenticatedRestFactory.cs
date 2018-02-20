﻿using Discord_UWP.API.User;
using Discord_UWP.API.Channel;
using Discord_UWP.Authentication;
using Refit;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord_UWP.API.Guild;
using Discord_UWP.API.Voice;
using Discord_UWP.API.Invite;
using Discord_UWP.API.Game;

namespace Discord_UWP.API
{
    public class AuthenticatedRestFactory
    {
        private readonly IAuthenticator _authenticator;
        private readonly DiscordApiConfiguration _apiConfig;
        
        public AuthenticatedRestFactory(DiscordApiConfiguration config, IAuthenticator authenticator)
        {
            _apiConfig = config;
            _authenticator = authenticator;
        }
        public IGameService GetGameService()
        {
            return RestService.For<IGameService>(GetAuthenticatingHttpClient());
        }

        public IUserService GetUserService()
        {
            return RestService.For<IUserService>(GetAuthenticatingHttpClient());
        }

        public IChannelService GetChannelService()
        {
            return RestService.For<IChannelService>(GetAuthenticatingHttpClient());
        }

        public IGuildService GetGuildService()
        {
            return RestService.For<IGuildService>(GetAuthenticatingHttpClient());
        }

        public IInviteService GetInviteService()
        {
            return RestService.For<IInviteService>(GetAuthenticatingHttpClient());
        }

        public IVoiceService GetVoiceService()
        {
            return RestService.For<IVoiceService>(GetAuthenticatingHttpClient());
        }

        public HttpClient GetAuthenticatingHttpClient()
        {
            return new HttpClient(GetAuthenticationHandler())
            {
                BaseAddress = new Uri(_apiConfig.BaseUrl)
            };
        }

        private HttpClientHandler GetAuthenticationHandler()
        {
            return new AuthenticatingHttpClientHandler(_authenticator);
        }
    }
}
