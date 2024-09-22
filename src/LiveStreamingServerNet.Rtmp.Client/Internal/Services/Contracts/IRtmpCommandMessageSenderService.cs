using LiveStreamingServerNet.Rtmp.Internal.Extensions;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts
{
    internal interface IRtmpCommandMessageSenderService
    {
        void SendCommandMessage(
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IReadOnlyDictionary<string, object>? commandObject,
            IReadOnlyList<object?>? parameters = null,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0,
            Action<bool>? callback = null);

        ValueTask SendCommandMessageAsync(
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IReadOnlyDictionary<string, object>? commandObject,
            IReadOnlyList<object?>? parameters = null,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0);
    }
}