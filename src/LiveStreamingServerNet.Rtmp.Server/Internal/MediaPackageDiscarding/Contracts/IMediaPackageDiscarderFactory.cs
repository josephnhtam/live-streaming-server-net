namespace LiveStreamingServerNet.Rtmp.Server.Internal.MediaPackageDiscarding.Contracts
{
    internal interface IMediaPackageDiscarderFactory
    {
        IMediaPackageDiscarder Create(uint clientId);
    }
}
