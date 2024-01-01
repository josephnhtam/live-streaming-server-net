using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.Rtmp.Services.Contracts
{
    public interface IRtmpUserControlMessageSenderService
    {
        void SendStreamBeginMessage(IRtmpClientPeerContext peerContext, uint publishStreamId);
        void SendStreamBeginMessage(IList<IRtmpClientPeerContext> peerContexts, uint publishStreamId);

        void SendStreamEofMessage(IRtmpClientPeerContext peerContext, uint publishStreamId);
        void SendStreamEofMessage(IList<IRtmpClientPeerContext> peerContexts, uint publishStreamId);
    }
}
