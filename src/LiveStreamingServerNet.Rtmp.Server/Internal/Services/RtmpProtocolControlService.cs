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
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ControlChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.SetChunkSize, RtmpConstants.ControlStreamId);

            _chunkMessageSenderService.Send(clientContext, basicHeader, messageHeader, dataBuffer =>
                dataBuffer.WriteUInt32BigEndian(chunkSize));

            clientContext.OutChunkSize = chunkSize;
        }

        public void AbortMessage(IRtmpClientSessionContext clientContext, uint streamId)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ControlChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.AbortMessage, RtmpConstants.ControlStreamId);

            _chunkMessageSenderService.Send(clientContext, basicHeader, messageHeader, dataBuffer =>
                dataBuffer.WriteUInt32BigEndian(streamId));
        }

        public void Acknowledgement(IRtmpClientSessionContext clientContext, uint sequenceNumber)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ControlChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.Acknowledgement, RtmpConstants.ControlStreamId);

            _chunkMessageSenderService.Send(clientContext, basicHeader, messageHeader, dataBuffer =>
                dataBuffer.WriteUInt32BigEndian(sequenceNumber));
        }

        public void WindowAcknowledgementSize(IRtmpClientSessionContext clientContext, uint windowAcknowledgementSize)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ControlChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.WindowAcknowledgementSize, RtmpConstants.ControlStreamId);

            _chunkMessageSenderService.Send(clientContext, basicHeader, messageHeader, dataBuffer =>
            {
                dataBuffer.WriteUInt32BigEndian(windowAcknowledgementSize);
            });

            clientContext.OutWindowAcknowledgementSize = windowAcknowledgementSize;
        }

        public void SetPeerBandwidth(IRtmpClientSessionContext clientContext, uint peerBandwidth, RtmpBandwidthLimitType limitType)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ControlChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.SetPeerBandwidth, RtmpConstants.ControlStreamId);

            _chunkMessageSenderService.Send(clientContext, basicHeader, messageHeader, dataBuffer =>
            {
                dataBuffer.WriteUInt32BigEndian(peerBandwidth);
                dataBuffer.Write((byte)limitType);
            });
        }
    }
}
