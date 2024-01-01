using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.Rtmp.Services.Contracts
{
    public interface IRtmpUserControlMessageSenderService
    {
        void SendStreamBeginMessage(IRtmpClientPeerContext peerContext);
        void SendStreamBeginMessage(IList<IRtmpClientPeerContext> peerContexts);

        void SendStreamEofMessage(IRtmpClientPeerContext peerContext);
        void SendStreamEofMessage(IList<IRtmpClientPeerContext> peerContexts);
    }
}
