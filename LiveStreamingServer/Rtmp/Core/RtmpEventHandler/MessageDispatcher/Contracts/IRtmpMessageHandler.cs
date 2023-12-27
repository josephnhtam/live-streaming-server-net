using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;
namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Contracts
{
    public interface IRtmpMessageHandler
    {
        Task<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, RtmpChunkEvent message, INetBuffer payloadBuffer, CancellationToken cancellationToken);
    }
}
