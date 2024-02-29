using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services
{
    internal class RtmpCommandMessageSenderService : IRtmpCommandMessageSenderService
    {
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;

        public RtmpCommandMessageSenderService(IRtmpChunkMessageSenderService chunkMessageSender)
        {
            _chunkMessageSender = chunkMessageSender;
        }

        public void SendCommandMessage(
            IRtmpClientContext clientContext,
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IDictionary<string, object>? commandObject,
            IList<object?> initialParameters,
            AmfEncodingType amfEncodingType,
            Action? callback)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, chunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0,
                amfEncodingType == AmfEncodingType.Amf0 ? RtmpMessageType.CommandMessageAmf0 : RtmpMessageType.CommandMessageAmf3, 0);

            _chunkMessageSender.Send(clientContext, basicHeader, messageHeader, netBuffer =>
            {
                var parameters = GetParameters(commandName, transactionId, commandObject, initialParameters);
                netBuffer.WriteAmf(parameters, amfEncodingType);
            }, callback);
        }

        public Task SendCommandMessageAsync(IRtmpClientContext clientContext, uint chunkStreamId, string commandName, double transactionId, IDictionary<string, object>? commandObject, IList<object?> parameters, AmfEncodingType amfEncodingType = AmfEncodingType.Amf0)
        {
            var tcs = new TaskCompletionSource();
            SendCommandMessage(clientContext, chunkStreamId, commandName, transactionId, commandObject, parameters, amfEncodingType, tcs.SetResult);
            return tcs.Task;
        }

        public void SendCommandMessage(
            IList<IRtmpClientContext> clientContexts,
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IDictionary<string, object>? commandObject,
            IList<object?> initialParameters,
            AmfEncodingType amfEncodingType)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, chunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0,
                amfEncodingType == AmfEncodingType.Amf0 ? RtmpMessageType.CommandMessageAmf0 : RtmpMessageType.CommandMessageAmf3, 0);

            _chunkMessageSender.Send(clientContexts, basicHeader, messageHeader, netBuffer =>
            {
                var parameters = GetParameters(commandName, transactionId, commandObject, initialParameters);
                netBuffer.WriteAmf(parameters, amfEncodingType);
            });
        }

        static List<object?> GetParameters(string commandName, double transactionId,
            IDictionary<string, object>? commandObject, IList<object?> initialParameters)
        {
            var additionalParameters = new List<object?>
            {
                commandName,
                transactionId,
                commandObject
            };

            return additionalParameters.Concat(initialParameters).ToList();
        }
    }
}
