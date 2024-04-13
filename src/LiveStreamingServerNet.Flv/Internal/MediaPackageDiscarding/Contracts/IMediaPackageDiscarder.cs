namespace LiveStreamingServerNet.Flv.Internal.MediaPackageDiscarding.Contracts
{
    internal interface IMediaPackageDiscarder
    {
        bool ShouldDiscardMediaPackage(bool isDiscardable, long outstandingSize, long outstandingCount);
    }
}
