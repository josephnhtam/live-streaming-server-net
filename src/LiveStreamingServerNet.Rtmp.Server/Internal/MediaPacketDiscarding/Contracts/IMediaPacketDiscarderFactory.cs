namespace LiveStreamingServerNet.Rtmp.Server.Internal.MediaPacketDiscarding.Contracts
{
    internal interface IMediaPacketDiscarderFactory
    {
        IMediaPacketDiscarder Create(uint clientId);
    }
}
