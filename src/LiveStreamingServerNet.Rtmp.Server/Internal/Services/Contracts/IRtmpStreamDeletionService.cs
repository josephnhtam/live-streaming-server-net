using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts
{
    internal interface IRtmpStreamDeletionService
    {
        ValueTask DeleteStreamAsync(IRtmpStream stream);
    }
}
