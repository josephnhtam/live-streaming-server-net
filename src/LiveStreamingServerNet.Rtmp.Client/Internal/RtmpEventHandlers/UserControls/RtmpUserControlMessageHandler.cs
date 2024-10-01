using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.RtmpEventHandlers.UserControls
{
    [RtmpMessageType(RtmpMessageType.UserControlMessage)]
    internal class RtmpUserControlMessageHandler : IRtmpMessageHandler<IRtmpSessionContext>
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;

        public RtmpUserControlMessageHandler(IRtmpChunkMessageSenderService chunkMessageSender)
        {
            _chunkMessageSender = chunkMessageSender;
        }

        public ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpSessionContext context,
            IDataBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            var eventType = payloadBuffer.ReadUInt16BigEndian();

            switch (eventType)
            {
                case RtmpUserControlMessageTypes.StreamBegin:
                case RtmpUserControlMessageTypes.StreamEOF:
                case RtmpUserControlMessageTypes.StreamDry:
                case RtmpUserControlMessageTypes.StreamIsRecorded:
                    {
                        HandleStreamEventType(context, (UserControlEventType)eventType, payloadBuffer);
                        break;
                    }

                case RtmpUserControlMessageTypes.PingRequest:
                    {
                        HandlePingRequest(payloadBuffer);
                        break;
                    }

                default:
                    break;
            }

            return ValueTask.FromResult(true);
        }

        private void HandleStreamEventType(IRtmpSessionContext context, UserControlEventType eventType, IDataBuffer payloadBuffer)
        {
            var streamId = payloadBuffer.ReadUInt32BigEndian();

            var subscribeStreamContext = context.GetStreamContext(streamId)?.SubscribeContext;

            if (subscribeStreamContext != null)
            {
                subscribeStreamContext.ReceiveUserControlEvent(new(eventType));
            }
        }

        private void HandlePingRequest(IDataBuffer payloadBuffer)
        {
            var timestamp = payloadBuffer.ReadUInt32BigEndian();

            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.ControlChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0, RtmpMessageType.UserControlMessage, RtmpConstants.ControlStreamId);

            _chunkMessageSender.Send(basicHeader, messageHeader, dataBuffer =>
            {
                dataBuffer.WriteUint16BigEndian(RtmpUserControlMessageTypes.PingResponse);
                dataBuffer.WriteUInt32BigEndian(timestamp);
            });
        }
    }
}
