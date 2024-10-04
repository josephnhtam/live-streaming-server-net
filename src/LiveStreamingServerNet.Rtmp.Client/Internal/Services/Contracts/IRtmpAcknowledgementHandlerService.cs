using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts
{
    internal interface IRtmpAcknowledgementHandlerService
    {
        void Handle(IRtmpSessionContext context, int receivedBytes);
    }
}