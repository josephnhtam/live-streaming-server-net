namespace LiveStreamingServerNet.Rtmp.Client.Internal.Contracts
{
    internal interface IRtmpStreamContext : IDisposable
    {
        uint StreamId { get; }

        IRtmpSessionContext SessionContext { get; }
    }
}
