using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.Extensions;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler;
using LiveStreamingServer.Rtmp.Core.RtmpHeaders;
using LiveStreamingServer.Rtmp.Core.Services.Contracts;

namespace LiveStreamingServer.Rtmp.Core.Services
{
    public class RtmpControlMessageSenderService
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSenderService;

        public RtmpControlMessageSenderService(IRtmpChunkMessageSenderService chunkMessageSenderService)
        {
            _chunkMessageSenderService = chunkMessageSenderService;
        }

        public void SetChunkSize(IRtmpClientPeerContext peerContext, uint chunkSize)
        {
            if (chunkSize < peerContext.OutChunkSize)
            {
                peerContext.OutChunkSize = chunkSize;
            }

            _chunkMessageSenderService.Send(peerContext, netBuffer =>
            {
                var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType0(0, 4, RtmpMessageType.SetChunkSize, RtmpConstants.ProtocolControlMessageStreamId);

                basicHeader.Write(netBuffer);
                messageHeader.Write(netBuffer);
                netBuffer.WriteUInt32BigEndian(chunkSize);
            },
            () => peerContext.OutChunkSize = chunkSize);
        }

        public void AbortMessage(IRtmpClientPeerContext peerContext, uint streamId)
        {
            _chunkMessageSenderService.Send(peerContext, netBuffer =>
            {
                var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType0(0, 4, RtmpMessageType.AbortMessage, RtmpConstants.ProtocolControlMessageStreamId);

                basicHeader.Write(netBuffer);
                messageHeader.Write(netBuffer);
                netBuffer.WriteUInt32BigEndian(streamId);
            });
        }

        public void Acknowledgement(IRtmpClientPeerContext peerContext, uint sequenceNumber)
        {
            _chunkMessageSenderService.Send(peerContext, netBuffer =>
            {
                var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType0(0, 4, RtmpMessageType.Acknowledgement, RtmpConstants.ProtocolControlMessageStreamId);

                basicHeader.Write(netBuffer);
                messageHeader.Write(netBuffer);
                netBuffer.WriteUInt32BigEndian(sequenceNumber);
            });
        }

        public void WindowAcknowledgementSize(IRtmpClientPeerContext peerContext, uint acknowledgementWindowSize)
        {
            _chunkMessageSenderService.Send(peerContext, netBuffer =>
            {
                var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType0(0, 4, RtmpMessageType.WindowAcknowledgementSize, RtmpConstants.ProtocolControlMessageStreamId);

                basicHeader.Write(netBuffer);
                messageHeader.Write(netBuffer);
                netBuffer.WriteUInt32BigEndian(acknowledgementWindowSize);
            });
        }

        public void SetPeerBandwidth(IRtmpClientPeerContext peerContext, uint acknowledgementWindowSize, PeerBandwidthLimitType limitType)
        {
            _chunkMessageSenderService.Send(peerContext, netBuffer =>
            {
                var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType0(0, 4, RtmpMessageType.WindowAcknowledgementSize, RtmpConstants.ProtocolControlMessageStreamId);

                basicHeader.Write(netBuffer);
                messageHeader.Write(netBuffer);
                netBuffer.WriteUInt32BigEndian(acknowledgementWindowSize);
                netBuffer.Write((byte)limitType);
            });
        }
    }
}
