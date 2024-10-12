namespace LiveStreamingServerNet.Rtmp.Relay.Internal.Contracts
{
    internal interface IRtmpDownstreamProcess
    {
        string StreamPath { get; }

        Task RunAsync(CancellationToken cancellationToken);
    }
}
