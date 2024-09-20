using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts
{
    internal interface IRtmpCommandMessageSenderService
    {
        void SendCommandMessage(
            IRtmpSessionContext context,
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IReadOnlyDictionary<string, object>? commandObject,
            IReadOnlyList<object?> parameters,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0,
            Action<bool>? callback = null);

        ValueTask SendCommandMessageAsync(
            IRtmpSessionContext context,
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IReadOnlyDictionary<string, object>? commandObject,
            IReadOnlyList<object?> parameters,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0);
    }
}