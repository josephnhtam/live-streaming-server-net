using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;

namespace LiveStreamingServer.Rtmp.Core.RtmpMessagetHandler.PayloadHandling.Contracts
{
    public interface IRtmpMessageDispatcher
    {
        Task<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, RtmpChunkEvent message, CancellationToken cancellationToken);
    }
}
