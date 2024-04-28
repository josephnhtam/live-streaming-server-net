using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Rtmp;

namespace LiveStreamingServerNet.Flv.Internal.Services.Contracts
{
    internal interface IFlvMediaTagSenderService
    {
        ValueTask SendMediaTagAsync(
            IFlvClient client,
            MediaType type,
            byte[] payloadBuffer,
            int payloadSize,
            uint timestamp,
            CancellationToken cancellation);
    }
}
