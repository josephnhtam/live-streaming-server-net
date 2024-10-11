namespace LiveStreamingServerNet.Flv.Internal.MediaPacketDiscarding.Contracts
{
    internal interface IMediaPacketDiscarderFactory
    {
        IMediaPacketDiscarder Create(string clientId);
    }
}
