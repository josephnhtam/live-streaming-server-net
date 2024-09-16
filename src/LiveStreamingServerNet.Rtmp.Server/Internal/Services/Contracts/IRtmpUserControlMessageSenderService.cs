using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts
{
    internal interface IRtmpUserControlMessageSenderService
    {
        void SendStreamBeginMessage(IRtmpClientSessionContext clientContext);
        void SendStreamBeginMessage(IReadOnlyList<IRtmpClientSessionContext> clientContexts);

        void SendStreamEofMessage(IRtmpClientSessionContext clientContext);
        void SendStreamEofMessage(IReadOnlyList<IRtmpClientSessionContext> clientContexts);
    }
}
