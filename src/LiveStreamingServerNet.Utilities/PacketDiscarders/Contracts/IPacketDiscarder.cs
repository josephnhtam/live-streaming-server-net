namespace LiveStreamingServerNet.Utilities.PacketDiscarders.Contracts
{
    public interface IPacketDiscarder
    {
        bool ShouldDiscardPacket(bool isDiscardable, long outstandingSize, long outstandingCount);
    }
}
