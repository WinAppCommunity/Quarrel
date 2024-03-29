﻿// Quarrel © 2022

using Discord.API.Exceptions;
using Discord.API.Voice.Models;
using Discord.API.Voice.Models.Handshake;
using System;

namespace Discord.API.Voice
{
    internal partial class VoiceConnection
    {
        private Action<VoiceConnectionStatus> VoiceConnectionStatusChanged { get; }
        private Action<SocketFrameException> UnhandledMessageEncountered { get; }
        private Action<VoiceOperation> KnownOperationEncountered { get; }
        private Action<int> UnknownOperationEncountered { get; }
        private Action<VoiceOperation> UnhandledOperationEncountered { get; }
        
        private Action<VoiceReady> Ready { get; }

        private Action<VoiceSessionDescription> SessionDescription { get; }
        private Action<Speaker> Speaking { get; }
        private Action<Video> Video { get; }

        private static bool FireEvent<T>(VoiceSocketFrame frame, Action<T> eventHandler)
        {
            var eventArgs = ((VoiceSocketFrame<T>)frame).Payload;
            eventHandler(eventArgs);
            return true;
        }

        public static bool FireEvent<T>(T data, Action<T> eventHandler)
        {
            eventHandler(data);
            return true;
        }
        
        protected void ProcessEvents(VoiceSocketFrame frame)
        {
            bool succeeded = frame switch
            {
                UnknownOperationVoiceSocketFrame osf => FireEvent(osf.Operation, UnknownOperationEncountered),
                _ => frame.Operation switch
                {
                    VoiceOperation.Hello => OnHello((VoiceSocketFrame<VoiceHello>)frame),
                    VoiceOperation.Ready => OnReady((VoiceSocketFrame<VoiceReady>)frame),
                    VoiceOperation.SessionDescription => FireEvent(frame, SessionDescription),
                    VoiceOperation.Heartbeat => OnHeartbeatAck(),
                    VoiceOperation.Speaking => FireEvent(frame, Speaking),
                    VoiceOperation.Video => FireEvent(frame, Video),

                    _ => FireEvent(frame.Operation, UnhandledOperationEncountered),
                }
            };
            if (!succeeded) FireEvent(new SocketFrameException("Failed to handle socket frame.", (int?)frame.Operation), UnhandledMessageEncountered);
        }
    }
}
