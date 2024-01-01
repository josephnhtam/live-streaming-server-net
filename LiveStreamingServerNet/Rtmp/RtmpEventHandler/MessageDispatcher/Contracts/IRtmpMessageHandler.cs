using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandler.MessageDispatcher.Contracts
{
    public interface IRtmpMessageHandler
    {
        Task<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientPeerContext peerContext, INetBuffer payloadBuffer, CancellationToken cancellationToken);
    }
}
