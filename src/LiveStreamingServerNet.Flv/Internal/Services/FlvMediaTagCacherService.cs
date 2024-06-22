using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class FlvMediaTagCacherService : IFlvMediaTagCacherService
    {
        private readonly IFlvMediaTagSenderService _mediaTagSender;

        public FlvMediaTagCacherService(IFlvMediaTagSenderService mediaTagSender)
        {
            _mediaTagSender = mediaTagSender;
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
            streamContext.GroupOfPicturesCache.Add(new PictureCacheInfo(mediaType, timestamp), rentedBuffer.Buffer, 0, rentedBuffer.Size);
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
                await _mediaTagSender.SendMediaTagAsync(client, picture.Type, picture.Payload.Buffer, picture.Payload.Size, picture.Timestamp, cancellation);
                picture.Payload.Unclaim();
            }
        }

        public async ValueTask SendCachedHeaderTagsAsync(IFlvClient client, IFlvStreamContext streamContext, uint timestamp, CancellationToken cancellation)
        {
            var audioSequenceHeader = streamContext.AudioSequenceHeader;
            if (audioSequenceHeader != null)
            {
                await _mediaTagSender.SendMediaTagAsync(client, MediaType.Audio, audioSequenceHeader, audioSequenceHeader.Length, timestamp, cancellation);
            }

            var videoSequenceHeader = streamContext.VideoSequenceHeader;
            if (videoSequenceHeader != null)
            {
                await _mediaTagSender.SendMediaTagAsync(client, MediaType.Video, videoSequenceHeader, videoSequenceHeader.Length, timestamp, cancellation);
            }
        }

        public ValueTask SendCachedMetaDataTagAsync(IFlvClient client, IFlvStreamContext streamContext, uint timestamp, CancellationToken cancellation)
        {
            return ValueTask.CompletedTask;
        }
    }
}
