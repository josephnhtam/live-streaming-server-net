﻿using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts
{
    internal interface IRtmpMediaCachingInterceptionService
    {
        ValueTask CacheSequenceHeaderAsync(IRtmpPublishStreamContext publishStreamContext, MediaType mediaType, byte[] sequenceHeader);
        ValueTask CachePictureAsync(IRtmpPublishStreamContext publishStreamContext, MediaType mediaType, IDataBuffer payloadBuffer, uint timestamp);
        ValueTask ClearGroupOfPicturesCacheAsync(IRtmpPublishStreamContext publishStreamContext);
    }
}
