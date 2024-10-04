using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts
{
    internal interface IRtmpAcknowledgementHandlerService
    {
        void Handle(IRtmpClientSessionContext clientContext, int receivedBytes);
    }
}