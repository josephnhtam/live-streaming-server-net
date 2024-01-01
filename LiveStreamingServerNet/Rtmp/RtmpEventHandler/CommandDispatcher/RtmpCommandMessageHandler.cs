using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.CommandDispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.MessageDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.MessageDispatcher.Contracts;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandler.CommandDispatcher
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
