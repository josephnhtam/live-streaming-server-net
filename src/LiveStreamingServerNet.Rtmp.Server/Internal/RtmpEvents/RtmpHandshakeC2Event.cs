﻿using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using Mediator;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEvents
{
    internal record RtmpHandshakeC2Event(
        IRtmpClientSessionContext ClientContext,
        INetworkStreamReader NetworkStream) : IRequest<RtmpEventConsumingResult>;
}
