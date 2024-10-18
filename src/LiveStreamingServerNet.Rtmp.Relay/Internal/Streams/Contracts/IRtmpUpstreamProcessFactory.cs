using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal.Streams.Contracts
{
    internal interface IRtmpUpstreamProcessFactory
    {
        IRtmpUpstreamProcess Create(IRtmpPublishStreamContext publishStreamContext);
    }
}
