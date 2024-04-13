namespace LiveStreamingServerNet.Flv.Internal.MediaPackageDiscarding.Contracts
{
    internal interface IMediaPackageDiscarderFactory
    {
        IMediaPackageDiscarder Create(string clientId);
    }
}
