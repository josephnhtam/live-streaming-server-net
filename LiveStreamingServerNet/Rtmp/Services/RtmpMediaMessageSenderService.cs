using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler;
using LiveStreamingServerNet.Rtmp.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Services
{
    public class RtmpMediaMessageSenderService : IRtmpMediaMessageSenderService
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;

        public RtmpMediaMessageSenderService(IRtmpChunkMessageSenderService chunkMessageSender)
        {
            _chunkMessageSender = chunkMessageSender;
        }

        public void SendAudioMessage(IRtmpClientPeerContext subscriber, IRtmpChunkStreamContext chunkStreamContext, Action<INetBuffer> payloadWriter, Action? callback = null)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.AudioMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(chunkStreamContext.MessageHeader.Timestamp,
                RtmpMessageType.AudioMessage, chunkStreamContext.MessageHeader.MessageStreamId);

            _chunkMessageSender.Send(subscriber, basicHeader, messageHeader, payloadWriter);
        }

        public void SendAudioMessage(IList<IRtmpClientPeerContext> subscribers, IRtmpChunkStreamContext chunkStreamContext, Action<INetBuffer> payloadWriter)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.AudioMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(chunkStreamContext.MessageHeader.Timestamp,
                RtmpMessageType.AudioMessage, chunkStreamContext.MessageHeader.MessageStreamId);

            _chunkMessageSender.Send(subscribers, basicHeader, messageHeader, payloadWriter);
        }

        public Task SendAudioMessageAsync(IRtmpClientPeerContext subscriber, IRtmpChunkStreamContext chunkStreamContext, Action<INetBuffer> payloadWriter)
        {
            var tcs = new TaskCompletionSource();
            SendAudioMessage(subscriber, chunkStreamContext, payloadWriter, tcs.SetResult);
            return tcs.Task;
        }

        public void SendVideoMessage(IRtmpClientPeerContext subscriber, IRtmpChunkStreamContext chunkStreamContext, Action<INetBuffer> payloadWriter, Action? callback = null)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.VideoMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(chunkStreamContext.MessageHeader.Timestamp,
                RtmpMessageType.VideoMessage, chunkStreamContext.MessageHeader.MessageStreamId);

            _chunkMessageSender.Send(subscriber, basicHeader, messageHeader, payloadWriter);
        }

        public void SendVideoMessage(IList<IRtmpClientPeerContext> subscribers, IRtmpChunkStreamContext chunkStreamContext, Action<INetBuffer> payloadWriter)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.VideoMessageChunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(chunkStreamContext.MessageHeader.Timestamp,
                RtmpMessageType.VideoMessage, chunkStreamContext.MessageHeader.MessageStreamId);

            _chunkMessageSender.Send(subscribers, basicHeader, messageHeader, payloadWriter);
        }

        public Task SendVideoMessageAsync(IRtmpClientPeerContext subscriber, IRtmpChunkStreamContext chunkStreamContext, Action<INetBuffer> payloadWriter)
        {
            var tcs = new TaskCompletionSource();
            SendVideoMessage(subscriber, chunkStreamContext, payloadWriter, tcs.SetResult);
            return tcs.Task;
        }
    }
}
