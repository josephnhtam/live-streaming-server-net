using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Extensions;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Services
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
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IDictionary<string, object>? commandObject,
            IList<object?> parameters,
            AmfEncodingType amfEncodingType,
            Action? callback)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, chunkStreamId);
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

        public Task SendCommandMessageAsync(IRtmpClientPeerContext peerContext, uint chunkStreamId, string commandName, double transactionId, IDictionary<string, object>? commandObject, IList<object?> parameters, AmfEncodingType amfEncodingType = AmfEncodingType.Amf0)
        {
            var tcs = new TaskCompletionSource();
            SendCommandMessage(peerContext, chunkStreamId, commandName, transactionId, commandObject, parameters, amfEncodingType, tcs.SetResult);
            return tcs.Task;
        }
    }
}
