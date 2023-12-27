using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher.Contracts
{
    public interface IRtmpCommandDispatcher
    {
        Task<bool> DispatchAsync(IRtmpChunkStreamContext chunkStreamContext, RtmpChunkEvent message, INetBuffer payloadBuffer, CancellationToken cancellationToken);
    }
}