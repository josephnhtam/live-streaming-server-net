﻿using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using MediatR;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEvents
{
    internal record struct RtmpChunkEvent(
        IRtmpClientContext ClientContext,
        ReadOnlyStream NetworkStream) : IRequest<RtmpEventConsumingResult>;
}
