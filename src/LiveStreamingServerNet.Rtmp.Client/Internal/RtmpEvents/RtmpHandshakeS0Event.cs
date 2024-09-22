﻿using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using Mediator;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEvents
{
    internal record RtmpHandshakeS0Event(
        IRtmpSessionContext Context, INetworkStreamReader NetworkStream) : IRequest<RtmpEventConsumingResult>;
}