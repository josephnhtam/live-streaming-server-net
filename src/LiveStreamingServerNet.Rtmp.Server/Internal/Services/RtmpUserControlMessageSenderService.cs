using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpUserControlMessageSenderService : IRtmpUserControlMessageSenderService
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSenderService;

        public RtmpUserControlMessageSenderService(IRtmpChunkMessageSenderService chunkMessageSenderService)
        {
            _chunkMessageSenderService = chunkMessageSenderService;
        }

        public void SendStreamBeginMessage(IRtmpSubscribeStreamContext subscribeStreamContext)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ControlChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.UserControlMessage, RtmpConstants.ControlStreamId);

            _chunkMessageSenderService.Send(subscribeStreamContext.StreamContext.ClientContext, basicHeader, messageHeader, dataBuffer =>
            {
                dataBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamBegin);
                dataBuffer.WriteUInt32BigEndian(subscribeStreamContext.StreamContext.StreamId);
            });
        }

        public void SendStreamBeginMessage(IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts)
        {
            foreach (var subscribeStreamContextGroup in subscribeStreamContexts.GroupBy(x => x.StreamContext.StreamId))
            {
                var streamId = subscribeStreamContextGroup.Key;
                var clientContexts = subscribeStreamContextGroup.Select(x => x.StreamContext.ClientContext).ToList();

                var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ControlChunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.UserControlMessage, RtmpConstants.ControlStreamId);

                _chunkMessageSenderService.Send(clientContexts, basicHeader, messageHeader, dataBuffer =>
                {
                    dataBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamBegin);
                    dataBuffer.WriteUInt32BigEndian(streamId);
                });
            }
        }

        public void SendStreamEofMessage(IRtmpSubscribeStreamContext subscribeStreamContext)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ControlChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.UserControlMessage, RtmpConstants.ControlStreamId);

            _chunkMessageSenderService.Send(subscribeStreamContext.StreamContext.ClientContext, basicHeader, messageHeader, dataBuffer =>
            {
                dataBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamEOF);
                dataBuffer.WriteUInt32BigEndian(subscribeStreamContext.StreamContext.StreamId);
            });
        }

        public void SendStreamEofMessage(IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts)
        {
            foreach (var subscribeStreamContextGroup in subscribeStreamContexts.GroupBy(x => x.StreamContext.StreamId))
            {
                var streamId = subscribeStreamContextGroup.Key;
                var clientContexts = subscribeStreamContextGroup.Select(x => x.StreamContext.ClientContext).ToList();

                var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ControlChunkStreamId);
                var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.UserControlMessage, RtmpConstants.ControlStreamId);

                _chunkMessageSenderService.Send(clientContexts, basicHeader, messageHeader, dataBuffer =>
                {
                    dataBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.StreamEOF);
                    dataBuffer.WriteUInt32BigEndian(streamId);
                });
            }
        }
    }
}
