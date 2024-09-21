using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpProtocolControlService : IRtmpProtocolControlService
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSenderService;

        public RtmpProtocolControlService(IRtmpChunkMessageSenderService chunkMessageSenderService)
        {
            _chunkMessageSenderService = chunkMessageSenderService;
        }

        public void SetChunkSize(IRtmpClientSessionContext clientContext, uint chunkSize)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.SetChunkSize, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(clientContext, basicHeader, messageHeader, dataBuffer =>
                dataBuffer.WriteUInt32BigEndian(chunkSize));

            clientContext.OutChunkSize = chunkSize;
        }

        public void AbortMessage(IRtmpClientSessionContext clientContext, uint streamId)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.AbortMessage, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(clientContext, basicHeader, messageHeader, dataBuffer =>
                dataBuffer.WriteUInt32BigEndian(streamId));
        }

        public void Acknowledgement(IRtmpClientSessionContext clientContext, uint sequenceNumber)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.Acknowledgement, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(clientContext, basicHeader, messageHeader, dataBuffer =>
                dataBuffer.WriteUInt32BigEndian(sequenceNumber));
        }

        public void WindowAcknowledgementSize(IRtmpClientSessionContext clientContext, uint windowAcknowledgementSize)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.WindowAcknowledgementSize, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(clientContext, basicHeader, messageHeader, dataBuffer =>
            {
                dataBuffer.WriteUInt32BigEndian(windowAcknowledgementSize);
            });

            clientContext.OutWindowAcknowledgementSize = windowAcknowledgementSize;
        }

        public void SetPeerBandwidth(IRtmpClientSessionContext clientContext, uint peerBandwidth, RtmpClientBandwidthLimitType limitType)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.SetClientBandwidth, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(clientContext, basicHeader, messageHeader, dataBuffer =>
            {
                dataBuffer.WriteUInt32BigEndian(peerBandwidth);
                dataBuffer.Write((byte)limitType);
            });
        }
    }
}
