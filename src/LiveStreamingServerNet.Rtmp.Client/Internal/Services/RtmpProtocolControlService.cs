using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using System.Diagnostics;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services
{
    internal class RtmpProtocolControlService : IRtmpProtocolControlService
    {
        private readonly IRtmpClientContext _clientContext;
        private readonly IRtmpChunkMessageSenderService _chunkMessageSenderService;

        public RtmpProtocolControlService(
            IRtmpClientContext clientContext,
            IRtmpChunkMessageSenderService chunkMessageSenderService)
        {
            _clientContext = clientContext;
            _chunkMessageSenderService = chunkMessageSenderService;
        }

        public void SetChunkSize(uint chunkSize)
        {
            Debug.Assert(_clientContext.SessionContext != null);

            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.SetChunkSize, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(basicHeader, messageHeader, dataBuffer =>
                dataBuffer.WriteUInt32BigEndian(chunkSize));

            _clientContext.SessionContext.OutChunkSize = chunkSize;
        }

        public void AbortMessage(uint streamId)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.AbortMessage, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(basicHeader, messageHeader, dataBuffer =>
                dataBuffer.WriteUInt32BigEndian(streamId));
        }

        public void Acknowledgement(uint sequenceNumber)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.Acknowledgement, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(basicHeader, messageHeader, dataBuffer =>
                dataBuffer.WriteUInt32BigEndian(sequenceNumber));
        }

        public void WindowAcknowledgementSize(uint acknowledgementWindowSize)
        {
            Debug.Assert(_clientContext.SessionContext != null);

            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.WindowAcknowledgementSize, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(basicHeader, messageHeader, dataBuffer =>
            {
                dataBuffer.WriteUInt32BigEndian(acknowledgementWindowSize);
            });

            _clientContext.SessionContext.OutWindowAcknowledgementSize = acknowledgementWindowSize;
        }

        public void SetPeerBandwidth(uint peerBandwidth, RtmpPeerBandwidthLimitType limitType)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ProtocolControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.SetPeerBandwidth, RtmpConstants.ProtocolControlMessageStreamId);

            _chunkMessageSenderService.Send(basicHeader, messageHeader, dataBuffer =>
            {
                dataBuffer.WriteUInt32BigEndian(peerBandwidth);
                dataBuffer.Write((byte)limitType);
            });
        }
    }
}
