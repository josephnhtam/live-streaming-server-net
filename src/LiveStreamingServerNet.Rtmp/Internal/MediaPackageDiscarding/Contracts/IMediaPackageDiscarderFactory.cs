namespace LiveStreamingServerNet.Rtmp.Internal.MediaPackageDiscarding.Contracts
{
    internal interface IMediaPackageDiscarderFactory
    {
        IMediaPackageDiscarder Create(uint clientId);
    }
}
