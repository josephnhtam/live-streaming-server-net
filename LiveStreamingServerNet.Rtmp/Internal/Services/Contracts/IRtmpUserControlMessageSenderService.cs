using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpUserControlMessageSenderService
    {
        void SendStreamBeginMessage(IRtmpClientPeerContext peerContext);
        void SendStreamBeginMessage(IList<IRtmpClientPeerContext> peerContexts);

        void SendStreamEofMessage(IRtmpClientPeerContext peerContext);
        void SendStreamEofMessage(IList<IRtmpClientPeerContext> peerContexts);
    }
}
