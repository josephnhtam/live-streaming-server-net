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

        public void SendStreamBeginMessage(IRtmpClientPeerContext peerContext, uint publishStreamId)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.UserControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.UserControlMessage, RtmpConstants.UserControlMessageStreamId);

            _chunkMessageSenderService.Send(peerContext, basicHeader, messageHeader, netBuffer =>
            {
                netBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamBegin);
                netBuffer.WriteUInt32BigEndian(publishStreamId);
            });
        }

        public void SendStreamBeginMessage(IList<IRtmpClientPeerContext> peerContexts, uint publishStreamId)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.UserControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.UserControlMessage, RtmpConstants.UserControlMessageStreamId);

            _chunkMessageSenderService.Send(peerContexts, basicHeader, messageHeader, netBuffer =>
            {
                netBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamBegin);
                netBuffer.WriteUInt32BigEndian(publishStreamId);
            });
        }

        public void SendStreamEofMessage(IRtmpClientPeerContext peerContext, uint publishStreamId)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.UserControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.UserControlMessage, RtmpConstants.UserControlMessageStreamId);

            _chunkMessageSenderService.Send(peerContext, basicHeader, messageHeader, netBuffer =>
            {
                netBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamEof);
                netBuffer.WriteUInt32BigEndian(publishStreamId);
            });
        }

        public void SendStreamEofMessage(IList<IRtmpClientPeerContext> peerContexts, uint publishStreamId)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.UserControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.UserControlMessage, RtmpConstants.UserControlMessageStreamId);

            _chunkMessageSenderService.Send(peerContexts, basicHeader, messageHeader, netBuffer =>
            {
                netBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamEof);
                netBuffer.WriteUInt32BigEndian(publishStreamId);
            });
        }
    }
}
