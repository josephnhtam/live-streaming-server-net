namespace LiveStreamingServerNet.Rtmp.Internal.MediaPackageDiscarding.Contracts
{
    internal interface IMediaPackageDiscarder
    {
        bool ShouldDiscardMediaPackage(bool isDiscardable, long outstandingSize, long outstandingCount);
    }
}
