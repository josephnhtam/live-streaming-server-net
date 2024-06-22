using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services
{
    internal class RtmpProtocolControlMessageSenderService : IRtmpProtocolControlMessageSenderService
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSenderService;

        public RtmpProtocolControlMessageSenderService(IRtmpChunkMessageSenderService chunkMessageSenderService)
        {
            _chunkMessageSenderService = chunkMessageSenderService;
        }

        public void SetChunkSize(IRtmpClientContext clientContext, uint chunkSize)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.SetChunkSize, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(clientContext, basicHeader, messageHeader, dataBuffer =>
                dataBuffer.WriteUInt32BigEndian(chunkSize));

            clientContext.OutChunkSize = chunkSize;
        }

        public void AbortMessage(IRtmpClientContext clientContext, uint streamId)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.AbortMessage, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(clientContext, basicHeader, messageHeader, dataBuffer =>
                dataBuffer.WriteUInt32BigEndian(streamId));
        }

        public void Acknowledgement(IRtmpClientContext clientContext, uint sequenceNumber)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.Acknowledgement, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(clientContext, basicHeader, messageHeader, dataBuffer =>
                dataBuffer.WriteUInt32BigEndian(sequenceNumber));
        }

        public void WindowAcknowledgementSize(IRtmpClientContext clientContext, uint acknowledgementWindowSize)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.WindowAcknowledgementSize, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(clientContext, basicHeader, messageHeader, dataBuffer =>
            {
                dataBuffer.WriteUInt32BigEndian(acknowledgementWindowSize);
            });

            clientContext.OutWindowAcknowledgementSize = acknowledgementWindowSize;
        }

        public void SetClientBandwidth(IRtmpClientContext clientContext, uint clientBandwidth, RtmpClientBandwidthLimitType limitType)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.SetClientBandwidth, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(clientContext, basicHeader, messageHeader, dataBuffer =>
            {
                dataBuffer.WriteUInt32BigEndian(clientBandwidth);
                dataBuffer.Write((byte)limitType);
            });
        }
    }
}
