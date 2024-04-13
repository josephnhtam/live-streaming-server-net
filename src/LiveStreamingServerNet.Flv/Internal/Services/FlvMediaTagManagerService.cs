using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.MediaPackageDiscarding.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal partial class FlvMediaTagManagerService : IFlvMediaTagManagerService, IAsyncDisposable
    {
        private readonly IMediaPackageDiscarderFactory _mediaPackageDiscarderFactory;
        private readonly ILogger _logger;

        public FlvMediaTagManagerService(
            IMediaPackageDiscarderFactory mediaPackageDiscarderFactory,
            ILogger<FlvMediaTagManagerService> logger)
        {
            _mediaPackageDiscarderFactory = mediaPackageDiscarderFactory;
            _logger = logger;
        }

        public ValueTask CacheSequenceHeaderAsync(IFlvStreamContext streamContext, MediaType mediaType, byte[] sequenceHeader)
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

            return ValueTask.CompletedTask;
        }

        public ValueTask CachePictureAsync(IFlvStreamContext streamContext, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp)
        {
            rentedBuffer.Claim();
            streamContext.GroupOfPicturesCache.Add(new PicturesCache(mediaType, timestamp, rentedBuffer));
            return ValueTask.CompletedTask;
        }

        public ValueTask ClearGroupOfPicturesCacheAsync(IFlvStreamContext streamContext)
        {
            streamContext.GroupOfPicturesCache.Clear();
            return ValueTask.CompletedTask;
        }

        public async ValueTask SendCachedGroupOfPicturesTagsAsync(IFlvClient client, IFlvStreamContext streamContext, CancellationToken cancellation)
        {
            foreach (var picture in streamContext.GroupOfPicturesCache.Get())
            {
                await SendMediaTagAsync(client, picture.Type, picture.Payload.Buffer, picture.Payload.Size, picture.Timestamp, cancellation);
                picture.Payload.Unclaim();
            }
        }

        public async ValueTask SendCachedHeaderTagsAsync(IFlvClient client, IFlvStreamContext streamContext, uint timestamp, CancellationToken cancellation)
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

        public ValueTask SendCachedMetaDataTagAsync(IFlvClient client, IFlvStreamContext streamContext, uint timestamp, CancellationToken cancellation)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask EnqueueMediaTagAsync(IFlvStreamContext streamContext, IReadOnlyList<IFlvClient> subscribers, MediaType mediaType, uint timestamp, bool isSkippable, IRentedBuffer rentedBuffer)
        {
            if (!subscribers.Any())
                return ValueTask.CompletedTask;

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

            return ValueTask.CompletedTask;
        }

        private async ValueTask SendMediaTagAsync(
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
