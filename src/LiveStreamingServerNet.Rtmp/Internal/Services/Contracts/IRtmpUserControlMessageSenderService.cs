using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpUserControlMessageSenderService
    {
        void SendStreamBeginMessage(IRtmpClientSessionContext clientContext);
        void SendStreamBeginMessage(IReadOnlyList<IRtmpClientSessionContext> clientContexts);

        void SendStreamEofMessage(IRtmpClientSessionContext clientContext);
        void SendStreamEofMessage(IReadOnlyList<IRtmpClientSessionContext> clientContexts);
    }
}
