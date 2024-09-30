﻿using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts
{
    internal interface IRtmpCommanderService
    {
        void Command(RtmpCommand command);
        void Command(RtmpCommand command, CommandCallbackDelegate callback, Action? cancellationCallback = null);
        void Connect(string appName, IDictionary<string, object>? information = null, ConnectCallbackDelegate? callback = null, Action? cancellationCallback = null);
        void CreateStream(CreateStreamCallbackDelegate? callback = null, Action? cancellationCallback = null);
        void CloseStream(uint streamId);
        void DeleteStream(uint streamId);
        void Play(uint streamId, string streamName, double start, double duration, bool reset);
    }

    internal delegate ValueTask ConnectCallbackDelegate(bool Success, IDictionary<string, object> CommandObject, object? Parameters);
    internal delegate ValueTask CreateStreamCallbackDelegate(bool Success, IRtmpStreamContext? StreamContext);
}
