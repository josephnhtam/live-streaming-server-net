using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts
{
    internal interface IRtmpStreamDeletionService
    {
        ValueTask CloseStreamAsync(IRtmpStreamContext streamContext);
        ValueTask DeleteStreamAsync(IRtmpStreamContext streamContext);
    }
}
