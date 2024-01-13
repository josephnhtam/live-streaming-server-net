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
            IDictionary<string, object>? commandObject,
            IList<object?> parameters,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0,
            Action? callback = null);

        Task SendCommandMessageAsync(
            IRtmpClientContext clientContext,
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IDictionary<string, object>? commandObject,
            IList<object?> parameters,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0);

        void SendCommandMessage(
           IList<IRtmpClientContext> clientContexts,
           uint chunkStreamId,
           string commandName,
           double transactionId,
           IDictionary<string, object>? commandObject,
           IList<object?> parameters,
           AmfEncodingType amfEncodingType = AmfEncodingType.Amf0);
    }
}
