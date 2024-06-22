using LiveStreamingServerNet.Rtmp.Internal.Contracts;
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

        public void SendStreamBeginMessage(IRtmpClientContext clientContext)
        {
            if (clientContext.StreamSubscriptionContext == null)
                return;

            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.UserControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.UserControlMessage, RtmpConstants.UserControlMessageStreamId);

            _chunkMessageSenderService.Send(clientContext, basicHeader, messageHeader, dataBuffer =>
            {
                dataBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamBegin);
                dataBuffer.WriteUInt32BigEndian(clientContext.StreamSubscriptionContext!.StreamId);
            });
        }

        public void SendStreamBeginMessage(IReadOnlyList<IRtmpClientContext> clientContexts)
        {
            foreach (var clientContextGroup in clientContexts.Where(x => x.StreamSubscriptionContext != null).GroupBy(x => x.StreamSubscriptionContext!.StreamId))
            {
                var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.UserControlMessageChunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.UserControlMessage, RtmpConstants.UserControlMessageStreamId);

                _chunkMessageSenderService.Send(clientContextGroup.ToList(), basicHeader, messageHeader, dataBuffer =>
                {
                    dataBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamBegin);
                    dataBuffer.WriteUInt32BigEndian(clientContextGroup.Key);
                });
            }
        }

        public void SendStreamEofMessage(IRtmpClientContext clientContext)
        {
            if (clientContext.StreamSubscriptionContext == null)
                return;

            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.UserControlMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.UserControlMessage, RtmpConstants.UserControlMessageStreamId);

            _chunkMessageSenderService.Send(clientContext, basicHeader, messageHeader, dataBuffer =>
            {
                dataBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamEof);
                dataBuffer.WriteUInt32BigEndian(clientContext.StreamSubscriptionContext!.StreamId);
            });
        }

        public void SendStreamEofMessage(IReadOnlyList<IRtmpClientContext> clientContexts)
        {
            foreach (var clientContextGroup in clientContexts.Where(x => x.StreamSubscriptionContext != null).GroupBy(x => x.StreamSubscriptionContext!.StreamId))
            {
                var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.UserControlMessageChunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.UserControlMessage, RtmpConstants.UserControlMessageStreamId);

                _chunkMessageSenderService.Send(clientContextGroup.ToList(), basicHeader, messageHeader, dataBuffer =>
                {
                    dataBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamEof);
                    dataBuffer.WriteUInt32BigEndian(clientContextGroup.Key);
                });
            }
        }
    }
}
