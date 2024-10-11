using LiveStreamingServerNet.Utilities.PacketDiscarders.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.MediaPacketDiscarders.Contracts
{
    internal interface IMediaPacketDiscarderFactory
    {
        IPacketDiscarder Create(uint clientId);
    }
}
