namespace LiveStreamingServerNet.Rtmp.Relay.Internal.Contracts
{
    internal interface IRtmpDownstreamProcessFactory
    {
        IRtmpDownstreamProcess Create(string streamPath);
    }
}
