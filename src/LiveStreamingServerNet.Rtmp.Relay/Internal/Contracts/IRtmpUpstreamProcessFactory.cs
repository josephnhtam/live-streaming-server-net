namespace LiveStreamingServerNet.Rtmp.Relay.Internal.Contracts
{
    internal interface IRtmpUpstreamProcessFactory
    {
        IRtmpUpstreamProcess Create(string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
