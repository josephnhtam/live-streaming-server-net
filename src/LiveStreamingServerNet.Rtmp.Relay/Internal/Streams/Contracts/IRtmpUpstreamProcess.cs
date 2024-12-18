﻿using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal.Streams.Contracts
{
    internal interface IRtmpUpstreamProcess : IAsyncDisposable
    {
        string StreamPath { get; }
        IReadOnlyDictionary<string, string> StreamArguments { get; }

        Task RunAsync(CancellationToken cancellationToken);
        void OnReceiveMediaData(MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable);
        void OnReceiveMetaData(IReadOnlyDictionary<string, object> metaData);
    }
}
