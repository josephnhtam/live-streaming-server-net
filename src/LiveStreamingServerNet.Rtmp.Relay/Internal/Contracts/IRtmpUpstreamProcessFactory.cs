using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal.Contracts
{
    internal interface IRtmpUpstreamProcessFactory
    {
        IRtmpUpstreamProcess Create(IRtmpPublishStreamContext publishStreamContext);
    }
}
