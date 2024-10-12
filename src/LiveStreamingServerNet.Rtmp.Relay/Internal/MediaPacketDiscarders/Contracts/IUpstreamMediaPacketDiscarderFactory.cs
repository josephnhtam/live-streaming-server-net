using LiveStreamingServerNet.Utilities.PacketDiscarders.Contracts;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal.MediaPacketDiscarders.Contracts
{
    internal interface IUpstreamMediaPacketDiscarderFactory
    {
        IPacketDiscarder Create(string streamPath);
    }
}
