using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Extensions;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Services
{
    public class RtmpUserControlMessageSenderService : IRtmpUserControlMessageSenderService
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSenderService;

        public RtmpUserControlMessageSenderService(IRtmpChunkMessageSenderService chunkMessageSenderService)
        {
            _chunkMessageSenderService = chunkMessageSenderService;
        }

        public void SendStreamBeginMessage(IRtmpClientPeerContext peerContext)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.UserControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.UserControlMessage, RtmpConstants.UserControlMessageStreamId);

            _chunkMessageSenderService.Send(peerContext, basicHeader, messageHeader, netBuffer =>
            {
                netBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamBegin);
                netBuffer.WriteUInt32BigEndian(peerContext.StreamSubscriptionContext!.ChunkStreamId);
            });
        }

        public void SendStreamBeginMessage(IList<IRtmpClientPeerContext> peerContexts)
        {
            foreach (var peerContextGroup in peerContexts.GroupBy(x => x.StreamSubscriptionContext!.ChunkStreamId))
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
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.UserControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.UserControlMessage, RtmpConstants.UserControlMessageStreamId);

            _chunkMessageSenderService.Send(peerContext, basicHeader, messageHeader, netBuffer =>
            {
                netBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamEof);
                netBuffer.WriteUInt32BigEndian(peerContext.StreamSubscriptionContext!.ChunkStreamId);
            });
        }

        public void SendStreamEofMessage(IList<IRtmpClientPeerContext> peerContexts)
        {
            foreach (var peerContextGroup in peerContexts.GroupBy(x => x.StreamSubscriptionContext!.ChunkStreamId))
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
