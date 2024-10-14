using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal.Contracts
{
    internal interface IRtmpDownstreamProcess : IAsyncDisposable
    {
        string StreamPath { get; }

        ValueTask<PublishingStreamResult> InitializeAsync(CancellationToken cancellationToken);
        Task RunAsync(CancellationToken cancellationToken);
    }
}
