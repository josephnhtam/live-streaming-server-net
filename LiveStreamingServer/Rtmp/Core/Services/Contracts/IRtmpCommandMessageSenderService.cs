using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.Extensions;

namespace LiveStreamingServer.Rtmp.Core.Services.Contracts
{
    public interface IRtmpCommandMessageSenderService
    {
        void SendCommandMessage(IRtmpClientPeerContext peerContext,
            uint streamId,
            string commandName,
            double transactionId,
            IDictionary<string, object>? commandObject,
            IList<object?> parameters,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0,
            Action? callback = null);

        Task SendCommandMessageAsync(IRtmpClientPeerContext peerContext,
            uint streamId,
            string commandName,
            double transactionId,
            IDictionary<string, object>? commandObject,
            IList<object?> parameters,
            AmfEncodingType amfEncodingType = AmfEncodingType.Amf0);
    }
}
