using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services
{
    internal class RtmpUserControlMessageSenderService : IRtmpUserControlMessageSenderService
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSenderService;

        public RtmpUserControlMessageSenderService(IRtmpChunkMessageSenderService chunkMessageSenderService)
        {
            _chunkMessageSenderService = chunkMessageSenderService;
        }

        public void SendStreamBeginMessage(IRtmpClientPeerContext peerContext)
        {
            if (peerContext.StreamSubscriptionContext == null)
                return;

            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.UserControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.UserControlMessage, RtmpConstants.UserControlMessageStreamId);

            _chunkMessageSenderService.Send(peerContext, basicHeader, messageHeader, netBuffer =>
            {
                netBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamBegin);
                netBuffer.WriteUInt32BigEndian(peerContext.StreamSubscriptionContext!.StreamId);
            });
        }

        public void SendStreamBeginMessage(IList<IRtmpClientPeerContext> peerContexts)
        {
            foreach (var peerContextGroup in peerContexts.Where(x => x.StreamSubscriptionContext != null).GroupBy(x => x.StreamSubscriptionContext!.StreamId))
            {
                var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.UserControlMessageChunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.UserControlMessage, RtmpConstants.UserControlMessageStreamId);

                _chunkMessageSenderService.Send(peerContextGroup.ToList(), basicHeader, messageHeader, netBuffer =>
                {
                    netBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamBegin);
                    netBuffer.WriteUInt32BigEndian(peerContextGroup.Key);
                });
            }
        }

        public void SendStreamEofMessage(IRtmpClientPeerContext peerContext)
        {
            if (peerContext.StreamSubscriptionContext == null)
                return;

            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.UserControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.UserControlMessage, RtmpConstants.UserControlMessageStreamId);

            _chunkMessageSenderService.Send(peerContext, basicHeader, messageHeader, netBuffer =>
            {
                netBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamEof);
                netBuffer.WriteUInt32BigEndian(peerContext.StreamSubscriptionContext!.StreamId);
            });
        }

        public void SendStreamEofMessage(IList<IRtmpClientPeerContext> peerContexts)
        {
            foreach (var peerContextGroup in peerContexts.Where(x => x.StreamSubscriptionContext != null).GroupBy(x => x.StreamSubscriptionContext!.StreamId))
            {
                var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.UserControlMessageChunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.UserControlMessage, RtmpConstants.UserControlMessageStreamId);

                _chunkMessageSenderService.Send(peerContextGroup.ToList(), basicHeader, messageHeader, netBuffer =>
                {
                    netBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamEof);
                    netBuffer.WriteUInt32BigEndian(peerContextGroup.Key);
                });
            }
        }
    }
}
