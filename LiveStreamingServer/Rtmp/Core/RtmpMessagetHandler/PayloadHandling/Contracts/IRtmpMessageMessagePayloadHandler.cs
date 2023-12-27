using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpMessages;

namespace LiveStreamingServer.Rtmp.Core.RtmpMessagetHandler.PayloadHandling.Contracts
{
    public interface IRtmpMessageMessagePayloadHandler
    {
        Task<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, RtmpChunkMessage message, CancellationToken cancellationToken);
    }
}
