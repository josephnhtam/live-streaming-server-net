﻿using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Transmuxer.Internal.Hls.Services.Contracts
{
    internal interface IHlsTransmuxerManager
    {
        bool RegisterTransmuxer(string streamPath, IHlsTransmuxer transmuxer);
        void UnregisterTransmuxer(string streamPath);

        ValueTask OnReceiveMediaMessage(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp);
    }
}