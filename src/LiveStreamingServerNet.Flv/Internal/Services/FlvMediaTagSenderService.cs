using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class FlvMediaTagSenderService : IFlvMediaTagSenderService
    {
        public async ValueTask SendMediaTagAsync(
            IFlvClient client,
            MediaType type,
            byte[] payloadBuffer,
            int payloadSize,
            uint timestamp,
            CancellationToken cancellation)
        {
            var flvTagType = type switch
            {
                MediaType.Video => FlvTagType.Video,
                MediaType.Audio => FlvTagType.Audio,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            await client.WriteTagAsync(flvTagType, timestamp,
                (dataBuffer) => dataBuffer.Write(payloadBuffer, 0, payloadSize), cancellation);
        }
    }
}
