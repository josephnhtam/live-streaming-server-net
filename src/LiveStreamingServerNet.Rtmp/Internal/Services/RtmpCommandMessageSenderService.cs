using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Common;

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
            IReadOnlyDictionary<string, object>? commandObject,
            IReadOnlyList<object?> parameters,
            AmfEncodingType amfEncodingType,
            Action<bool>? callback)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, chunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0,
                amfEncodingType == AmfEncodingType.Amf0 ? RtmpMessageType.CommandMessageAmf0 : RtmpMessageType.CommandMessageAmf3, 0);

            _chunkMessageSender.Send(clientContext, basicHeader, messageHeader, dataBuffer =>
            {
                var values = GetParameters(commandName, transactionId, commandObject, parameters);
                dataBuffer.WriteAmf(values, amfEncodingType);
            }, callback);
        }

        public ValueTask SendCommandMessageAsync(IRtmpClientContext clientContext, uint chunkStreamId, string commandName, double transactionId, IReadOnlyDictionary<string, object>? commandObject, IReadOnlyList<object?> parameters, AmfEncodingType amfEncodingType = AmfEncodingType.Amf0)
        {
            var tcs = new ValueTaskCompletionSource();
            SendCommandMessage(clientContext, chunkStreamId, commandName, transactionId, commandObject, parameters, amfEncodingType, _ => tcs.SetResult());
            return tcs.Task;
        }

        public void SendCommandMessage(
            IReadOnlyList<IRtmpClientContext> clientContexts,
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IReadOnlyDictionary<string, object>? commandObject,
            IReadOnlyList<object?> parameters,
            AmfEncodingType amfEncodingType)
        {
            var basicHeader = new RtmpChunkBasicHeader(0, chunkStreamId);
            var messageHeader = new RtmpChunkMessageHeaderType0(0,
                amfEncodingType == AmfEncodingType.Amf0 ? RtmpMessageType.CommandMessageAmf0 : RtmpMessageType.CommandMessageAmf3, 0);

            _chunkMessageSender.Send(clientContexts, basicHeader, messageHeader, dataBuffer =>
            {
                var values = GetParameters(commandName, transactionId, commandObject, parameters);
                dataBuffer.WriteAmf(values, amfEncodingType);
            });
        }

        static List<object?> GetParameters(string commandName, double transactionId,
            IReadOnlyDictionary<string, object>? commandObject, IReadOnlyList<object?> additionalParameters)
        {
            var parameters = new List<object?>
            {
                commandName,
                transactionId,
                commandObject
            };

            return parameters.Concat(additionalParameters).ToList();
        }
    }
}
