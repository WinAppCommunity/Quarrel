﻿// Quarrel © 2022

using System.Threading.Tasks;

namespace Discord.API.Sockets
{
    internal partial class DiscordSocketClient<TFrame, TOperation, TEvent>
    {
        private bool _receivedAck;

        public int LastEventSequenceNumber { get; private set; }

        protected bool OnHeartbeatAck()
        {
            _receivedAck = true;
            return true;
        }

        protected async Task BeginHeartbeatAsync(int interval)
        {
            while (ConnectionStatus == ConnectionStatus.Connected)
            {
                await SendHeartbeatAsync();
                _receivedAck = false;
                await Task.Delay(interval);
                if (!_receivedAck)
                {
                    ConnectionStatus = ConnectionStatus.Disconnected;
                    await CloseSocket();
                    await ResumeAsync();
                }
            }
        }

        protected abstract Task SendHeartbeatAsync();
    }
}
