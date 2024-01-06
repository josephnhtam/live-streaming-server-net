using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Extensions;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Services
{
    public class RtmpProtocolControlMessageSenderService : IRtmpProtocolControlMessageSenderService
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSenderService;

        public RtmpProtocolControlMessageSenderService(IRtmpChunkMessageSenderService chunkMessageSenderService)
        {
            _chunkMessageSenderService = chunkMessageSenderService;
        }

        public void SetChunkSize(IRtmpClientPeerContext peerContext, uint chunkSize)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.SetChunkSize, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(peerContext, basicHeader, messageHeader, netBuffer =>
            {
                netBuffer.WriteUInt32BigEndian(chunkSize);
            });

            peerContext.OutChunkSize = chunkSize;
        }

        public void AbortMessage(IRtmpClientPeerContext peerContext, uint streamId)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.AbortMessage, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(peerContext, basicHeader, messageHeader, netBuffer =>
            {
                netBuffer.WriteUInt32BigEndian(streamId);
            });
        }

        public void Acknowledgement(IRtmpClientPeerContext peerContext, uint sequenceNumber)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.Acknowledgement, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(peerContext, basicHeader, messageHeader, netBuffer =>
            {
                netBuffer.WriteUInt32BigEndian(sequenceNumber);
            });
        }

        public void WindowAcknowledgementSize(IRtmpClientPeerContext peerContext, uint acknowledgementWindowSize)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.WindowAcknowledgementSize, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(peerContext, basicHeader, messageHeader, netBuffer =>
            {
                netBuffer.WriteUInt32BigEndian(acknowledgementWindowSize);
            });

            peerContext.OutWindowAcknowledgementSize = acknowledgementWindowSize;
        }

        public void SetPeerBandwidth(IRtmpClientPeerContext peerContext, uint peerBandwidth, RtmpPeerBandwidthLimitType limitType)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.SetPeerBandwidth, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(peerContext, basicHeader, messageHeader, netBuffer =>
            {
                netBuffer.WriteUInt32BigEndian(peerBandwidth);
                netBuffer.Write((byte)limitType);
            });
        }
    }
}
