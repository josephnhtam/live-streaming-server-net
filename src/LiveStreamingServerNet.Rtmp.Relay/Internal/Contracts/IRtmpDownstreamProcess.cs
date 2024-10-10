namespace LiveStreamingServerNet.Rtmp.Relay.Internal.Contracts
{
    internal interface IRtmpDownstreamProcess
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}
