﻿// Quarrel © 2022

using Discord.API.Sockets;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Tasks;

namespace Discord.API.Voice
{
    internal partial class VoiceConnection
    {
        private readonly JsonSerializerOptions _serializeOptions;
        private readonly JsonSerializerOptions _deserializeOptions;
        private DiscordSocketClient<VoiceSocketFrame>? _socket;

        /// <summary>
        /// Sets up a connection to the voice connection.
        /// </summary>
        /// <param name="url">The url of the voice connection.</param>
        public async Task ConnectAsync(string url)
        {
            VoiceConnectionStatus = VoiceConnectionStatus == VoiceConnectionStatus.Initialized ? VoiceConnectionStatus.Connecting : VoiceConnectionStatus.Reconnecting;

            _connectionUrl = url;
            _socket = new DiscordSocketClient<VoiceSocketFrame>(_serializeOptions, _deserializeOptions, HandleMessage, HandleError, UnhandledMessageEncountered);
            await _socket.ConnectAsync(url);
            await IdentifySelfToVoiceConnection();
        }

        private async Task SendMessageAsync<T>(VoiceOperation op, T payload)
            => await SendMessageAsync(op, null, payload);

        private async Task SendMessageAsync<T>(VoiceOperation op, VoiceEvent? e, T payload)
        {
            var frame = new VoiceSocketFrame<T>
            {
                Operation = op,
                Event = e,
                Payload = payload,
            };

            await _socket!.SendMessageAsync(frame);
        }

        private void HandleMessage(VoiceSocketFrame frame)
        {
            if (frame.SequenceNumber.HasValue)
            {
                _lastEventSequenceNumber = frame.SequenceNumber.Value;
            }

            ProcessEvents(frame);
        }

        private void HandleError(WebSocketCloseStatus? status)
        {
            switch (status)
            {
                default:
                    VoiceConnectionStatus = VoiceConnectionStatus.Disconnected;
                    _socket = null;
                    return;

            }
        }
    }
}
