using LiveStreamingServerNet.Rtmp.Client.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Contracts
{
    internal interface IRtmpStreamFactory
    {
        IRtmpStream Create(RtmpClient client, IRtmpStreamContext context);
    }
}
