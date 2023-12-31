using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.Extensions;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler;
using LiveStreamingServer.Rtmp.Core.RtmpHeaders;
using LiveStreamingServer.Rtmp.Core.Services.Contracts;
using System.Transactions;

namespace LiveStreamingServer.Rtmp.Core.Services
{
    public class RtmpMediaMessageSenderService : IRtmpMediaMessageSenderService
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;

        public RtmpMediaMessageSenderService(IRtmpChunkMessageSenderService chunkMessageSender)
        {
            _chunkMessageSender = chunkMessageSender;
        }

        public void SendCommandMessage(
            IRtmpClientPeerContext peerContext,
            uint streamId,
            string commandName,
            double transactionId,
            IDictionary<string, object>? commandObject,
            IList<object?> parameters,
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
                    commandObject,
                    .. parameters
                ], amfEncodingType);
            }, callback);
        }

        public void SendAudioMessage<TRtmpChunkMessageHeader>(IRtmpClientPeerContext subscriber, Action<INetBuffer> payloadWriter, Action? callback = null)
        {
            throw new NotImplementedException();
        }

        public void SendAudioMessage<TRtmpChunkMessageHeader>(IList<IRtmpClientPeerContext> subscribers, Action<INetBuffer> payloadWriter)
        {
            throw new NotImplementedException();
        }

        public Task SendAudioMessageAsync<TRtmpChunkMessageHeader>(IRtmpClientPeerContext subscriber, Action<INetBuffer> payloadWriter)
        {
            var tcs = new TaskCompletionSource();
            SendAudioMessage<TRtmpChunkMessageHeader>(subscriber, payloadWriter, tcs.SetResult);
            return tcs.Task;
        }

        public void SendVideoMessage<TRtmpChunkMessageHeader>(IRtmpClientPeerContext subscriber, Action<INetBuffer> payloadWriter, Action? callback = null)
        {
            //var basicHeader = new RtmpChunkBasicHeader(0, RtmpConstants.VideoMessageChunkStreamId);
            //var messageHeader = new RtmpChunkMessageHeaderType0(chunkStreamContext.MessageHeader.Timestamp,
            //    RtmpMessageType.VideoMessage, chunkStreamContext.MessageHeader.MessageStreamId);
        }

        public void SendVideoMessage<TRtmpChunkMessageHeader>(IList<IRtmpClientPeerContext> subscribers, RtmpChunkBasicHeader basicHeader, TRtmpChunkMessageHeader messageHeader, Action<INetBuffer> payloadWriter)
        {
            throw new NotImplementedException();
        }

        public Task SendVideoMessageAsync<TRtmpChunkMessageHeader>(IRtmpClientPeerContext subscriber, Action<INetBuffer> payloadWriter)
        {
            var tcs = new TaskCompletionSource();
            SendVideoMessage<TRtmpChunkMessageHeader>(subscriber, payloadWriter, tcs.SetResult);
            return tcs.Task;
        }
    }
}
