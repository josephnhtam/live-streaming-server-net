using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Contracts
{
    public interface IRtmpMessageDispatcher
    {
        Task<bool> DispatchAsync(IRtmpChunkStreamContext chunkStreamContext, RtmpChunkEvent message, CancellationToken cancellationToken);
    }
}
