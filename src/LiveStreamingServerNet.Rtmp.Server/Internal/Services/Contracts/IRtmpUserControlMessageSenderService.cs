using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts
{
    internal interface IRtmpUserControlMessageSenderService
    {
        void SendStreamBeginMessage(IRtmpSubscribeStreamContext subscribeStreamContext);
        void SendStreamBeginMessage(IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts);

        void SendStreamEofMessage(IRtmpSubscribeStreamContext subscribeStreamContext);
        void SendStreamEofMessage(IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts);
    }
}
