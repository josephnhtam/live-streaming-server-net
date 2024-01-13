using LiveStreamingServerNet.Flv.Configurations;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal partial class FlvMediaTagManagerService : IFlvMediaTagManagerService, IAsyncDisposable
    {
        private readonly MediaMessageConfiguration _config;
        private readonly ILogger _logger;

        public FlvMediaTagManagerService(
            IOptions<MediaMessageConfiguration> config,
            ILogger<FlvMediaTagManagerService> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public Task CacheSequenceHeaderAsync(IFlvStreamContext streamContext, MediaType mediaType, byte[] sequenceHeader)
        {
            switch (mediaType)
            {
                case MediaType.Video:
                    streamContext.VideoSequenceHeader = sequenceHeader;
                    break;
                case MediaType.Audio:
                    streamContext.AudioSequenceHeader = sequenceHeader;
                    break;
            }

            return Task.CompletedTask;
        }

        public Task CachePictureAsync(IFlvStreamContext streamContext, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp)
        {
            rentedBuffer.Claim();
            streamContext.GroupOfPicturesCache.Add(new PicturesCache(mediaType, timestamp, rentedBuffer));
            return Task.CompletedTask;
        }

        public Task ClearGroupOfPicturesCacheAsync(IFlvStreamContext streamContext)
        {
            streamContext.GroupOfPicturesCache.Clear();
            return Task.CompletedTask;
        }

        public async Task SendCachedGroupOfPicturesTagsAsync(IFlvClient client, IFlvStreamContext streamContext, CancellationToken cancellation)
        {
            foreach (var picture in streamContext.GroupOfPicturesCache.Get())
            {
                await SendMediaTagAsync(client, picture.Type, picture.Payload.Buffer, picture.Payload.Size, picture.Timestamp, cancellation);
                picture.Payload.Unclaim();
            }
        }

        public async Task SendCachedHeaderTagsAsync(IFlvClient client, IFlvStreamContext streamContext, uint timestamp, CancellationToken cancellation)
        {
            var audioSequenceHeader = streamContext.AudioSequenceHeader;
            if (audioSequenceHeader != null)
            {
                await SendMediaTagAsync(client, MediaType.Audio, audioSequenceHeader, audioSequenceHeader.Length, timestamp, cancellation);
            }

            var videoSequenceHeader = streamContext.VideoSequenceHeader;
            if (videoSequenceHeader != null)
            {
                await SendMediaTagAsync(client, MediaType.Video, videoSequenceHeader, videoSequenceHeader.Length, timestamp, cancellation);
            }
        }

        public Task SendCachedMetaDataTagAsync(IFlvClient client, IFlvStreamContext streamContext, uint timestamp, CancellationToken cancellation)
        {
            return Task.CompletedTask;
        }

        public Task EnqueueMediaTagAsync(IFlvStreamContext streamContext, IList<IFlvClient> subscribers, MediaType mediaType, uint timestamp, bool isSkippable, IRentedBuffer rentedBuffer)
        {
            if (!subscribers.Any())
                return Task.CompletedTask;

            rentedBuffer.Claim(subscribers.Count);

            var mediaPackage = new ClientMediaPackage(
                mediaType,
                timestamp,
                rentedBuffer,
                isSkippable);

            foreach (var subscriber in subscribers)
            {
                var mediaContext = GetMediaContext(subscriber);
                if (mediaContext == null || !mediaContext.AddPackage(ref mediaPackage))
                    rentedBuffer.Unclaim();
            }

            return Task.CompletedTask;
        }

        private async Task SendMediaTagAsync(
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

            var flvTagHeader = new FlvTagHeader(flvTagType, (uint)payloadSize, timestamp);

            await client.FlvWriter.WriteTagAsync(flvTagHeader,
                (netBuffer) => netBuffer.Write(payloadBuffer, 0, payloadSize), cancellation);
        }
    }
}
