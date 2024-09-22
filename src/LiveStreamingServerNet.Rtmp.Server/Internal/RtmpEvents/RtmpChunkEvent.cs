﻿using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using Mediator;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEvents
{
    internal class RtmpChunkEvent : IRequest<RtmpEventConsumingResult>
    {
        public IRtmpClientSessionContext ClientContext { get; set; } = default!;
        public INetworkStreamReader NetworkStream { get; set; } = default!;
    }
}
