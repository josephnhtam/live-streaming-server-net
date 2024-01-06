using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Contracts;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher
{
    [RtmpMessageType(RtmpMessageType.CommandMessageAmf0)]
    [RtmpMessageType(RtmpMessageType.CommandMessageAmf3)]
    public class RtmpCommandMessageHandler : IRtmpMessageHandler
    {
        private readonly IRtmpCommandDispatcher _dispatcher;

        public RtmpCommandMessageHandler(IRtmpCommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async Task<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientPeerContext peerContext, INetBuffer payloadBuffer, CancellationToken cancellationToken)
        {
            return await _dispatcher.DispatchAsync(chunkStreamContext, peerContext, payloadBuffer, cancellationToken);
        }
    }
}
