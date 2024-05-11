using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpCommandMessageSenderService
    {
        void SendCommandMessage(
            IRtmpClientContext clientContext,
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IReadOnlyDictionary<string, object>? commandObject,
            IReadOnlyList<object?> parameters,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0,
            Action<bool>? callback = null);

        ValueTask SendCommandMessageAsync(
            IRtmpClientContext clientContext,
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IReadOnlyDictionary<string, object>? commandObject,
            IReadOnlyList<object?> parameters,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0);

        void SendCommandMessage(
           IReadOnlyList<IRtmpClientContext> clientContexts,
           uint chunkStreamId,
           string commandName,
           double transactionId,
           IReadOnlyDictionary<string, object>? commandObject,
           IReadOnlyList<object?> parameters,
           AmfEncodingType amfEncodingType = AmfEncodingType.Amf0);
    }
}
