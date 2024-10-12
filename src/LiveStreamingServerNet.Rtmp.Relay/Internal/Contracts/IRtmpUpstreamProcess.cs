using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal.Contracts
{
    internal interface IRtmpUpstreamProcess
    {
        string StreamPath { get; }
        IReadOnlyDictionary<string, string> StreamArguments { get; }

        Task RunAsync(CancellationToken cancellationToken);
        void OnReceiveMediaData(MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable);
    }
}
