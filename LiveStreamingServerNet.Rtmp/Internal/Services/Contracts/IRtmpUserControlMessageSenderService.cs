using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpUserControlMessageSenderService
    {
        void SendStreamBeginMessage(IRtmpClientContext clientContext);
        void SendStreamBeginMessage(IList<IRtmpClientContext> clientContexts);

        void SendStreamEofMessage(IRtmpClientContext clientContext);
        void SendStreamEofMessage(IList<IRtmpClientContext> clientContexts);
    }
}
