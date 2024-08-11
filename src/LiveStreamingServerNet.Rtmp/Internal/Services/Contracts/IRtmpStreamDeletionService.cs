using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpStreamDeletionService
    {
        ValueTask DeleteStreamAsync(IRtmpClientSessionContext clientContext);
    }
}
