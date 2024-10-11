using LiveStreamingServerNet.Utilities.PacketDiscarders.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.MediaPacketDiscarders.Contracts
{
    internal interface IMediaPacketDiscarderFactory
    {
        IPacketDiscarder Create(string clientId);
    }
}
