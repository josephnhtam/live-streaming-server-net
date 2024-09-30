using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts
{
    internal interface IRtmpCommandMessageSenderService
    {
        void SendCommandMessage(
            IRtmpClientSessionContext clientContext,
            uint messageStreamId,
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IReadOnlyDictionary<string, object>? commandObject,
            IReadOnlyList<object?>? parameters = null,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0,
            Action<bool>? callback = null);

        ValueTask SendCommandMessageAsync(
            IRtmpClientSessionContext clientContext,
            uint messageStreamId,
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IReadOnlyDictionary<string, object>? commandObject,
            IReadOnlyList<object?>? parameters = null,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0);

        void SendCommandMessage(
            IReadOnlyList<IRtmpClientSessionContext> clientContexts,
            uint messageStreamId,
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IReadOnlyDictionary<string, object>? commandObject,
            IReadOnlyList<object?>? parameters = null,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0);
    }
}