using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.Extensions;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler;
using LiveStreamingServer.Rtmp.Core.RtmpHeaders;
using LiveStreamingServer.Rtmp.Core.Services.Contracts;

namespace LiveStreamingServer.Rtmp.Core.Services
{
    public class RtmpCommandMessageSenderService : IRtmpCommandMessageSenderService
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;

        public RtmpCommandMessageSenderService(IRtmpChunkMessageSenderService chunkMessageSender)
        {
            _chunkMessageSender = chunkMessageSender;
        }

        public void SendCommandMessage(
            IRtmpClientPeerContext peerContext,
            uint streamId,
            string commandName,
            double transactionId,
            IList<object> parameters,
            AmfEncodingType amfEncodingType,
            Action? callback)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, streamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0,
                amfEncodingType == AmfEncodingType.Amf0 ? RtmpMessageType.CommandMessageAmf0 : RtmpMessageType.CommandMessageAmf3, 0);

            _chunkMessageSender.Send(peerContext, basicHeader, messageHeader, netBuffer =>
            {
                netBuffer.WriteAmf([
                    commandName,
                    transactionId,
                    .. parameters
                ], amfEncodingType);
            }, callback);
        }
    }
}
