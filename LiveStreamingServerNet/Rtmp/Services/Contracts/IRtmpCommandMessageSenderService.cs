using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Extensions;

namespace LiveStreamingServerNet.Rtmp.Services.Contracts
{
    public interface IRtmpCommandMessageSenderService
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
