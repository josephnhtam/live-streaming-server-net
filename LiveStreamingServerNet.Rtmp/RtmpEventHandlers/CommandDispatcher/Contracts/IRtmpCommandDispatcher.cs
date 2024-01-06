using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher.Contracts
{
    public interface IRtmpCommandDispatcher
    {
        Task<bool> DispatchAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientPeerContext peerContext, INetBuffer payloadBuffer, CancellationToken cancellationToken);
    }
}