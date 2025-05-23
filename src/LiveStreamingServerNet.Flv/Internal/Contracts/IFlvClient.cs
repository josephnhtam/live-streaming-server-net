﻿using LiveStreamingServerNet.Rtmp.Utilities.Containers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Contracts
{
    internal interface IFlvClient : IAsyncDisposable
    {
        string ClientId { get; }
        string StreamPath { get; }
        CancellationToken StoppingToken { get; }
        void Stop();
        void CompleteInitialization();
        Task UntilInitializationCompleteAsync(CancellationToken cancellationToken = default);
        Task UntilCompleteAsync(CancellationToken cancellationToken = default);
        ValueTask WriteHeaderAsync(bool allowAudioTags, bool allowVideoTags, CancellationToken cancellationToken);
        ValueTask WriteTagAsync(FlvTagType tagType, uint timestamp, Action<IDataBuffer> payloadBuffer, CancellationToken cancellationToken);
    }
}
