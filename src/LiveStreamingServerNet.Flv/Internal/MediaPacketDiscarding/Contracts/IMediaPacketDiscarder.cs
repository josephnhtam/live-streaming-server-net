namespace LiveStreamingServerNet.Flv.Internal.MediaPacketDiscarding.Contracts
{
    internal interface IMediaPacketDiscarder
    {
        bool ShouldDiscardMediaPacket(bool isDiscardable, long outstandingSize, long outstandingCount);
    }
}
