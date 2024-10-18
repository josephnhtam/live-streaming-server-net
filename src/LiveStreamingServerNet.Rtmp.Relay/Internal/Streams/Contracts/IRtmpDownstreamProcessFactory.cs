namespace LiveStreamingServerNet.Rtmp.Relay.Internal.Streams.Contracts
{
    internal interface IRtmpDownstreamProcessFactory
    {
        IRtmpDownstreamProcess Create(string streamPath);
    }
}
