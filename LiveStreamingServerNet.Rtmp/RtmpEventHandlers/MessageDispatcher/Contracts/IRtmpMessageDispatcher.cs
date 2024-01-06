using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Contracts
{
    public interface IRtmpMessageDispatcher
    {
        Task<bool> DispatchAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientPeerContext peerContext, CancellationToken cancellationToken);
    }
}
