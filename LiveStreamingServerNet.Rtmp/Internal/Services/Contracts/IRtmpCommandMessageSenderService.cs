using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpCommandMessageSenderService
    {
        void SendCommandMessage(
            IRtmpClientPeerContext peerContext,
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IDictionary<string, object>? commandObject,
            IList<object?> parameters,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0,
            Action? callback = null);

        Task SendCommandMessageAsync(
            IRtmpClientPeerContext peerContext,
            uint chunkStreamId,
            string commandName,
            double transactionId,
            IDictionary<string, object>? commandObject,
            IList<object?> parameters,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0);

        void SendCommandMessage(
           IList<IRtmpClientPeerContext> peerContexts,
           uint chunkStreamId,
           string commandName,
           double transactionId,
           IDictionary<string, object>? commandObject,
           IList<object?> parameters,
           AmfEncodingType amfEncodingType = AmfEncodingType.Amf0);
    }
}
