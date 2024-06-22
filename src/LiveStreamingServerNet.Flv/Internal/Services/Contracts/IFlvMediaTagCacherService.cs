using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services.Contracts
{
    internal interface IFlvMediaTagCacherService
    {
        ValueTask CacheSequenceHeaderAsync(
           IFlvStreamContext streamContext,
           MediaType mediaType,
           byte[] sequenceHeader);

        ValueTask CachePictureAsync(
            IFlvStreamContext streamContext,
            MediaType mediaType,
            IRentedBuffer rentedBuffer,
            uint timestamp);

        ValueTask ClearGroupOfPicturesCacheAsync(IFlvStreamContext streamContext);

        ValueTask SendCachedHeaderTagsAsync(
            IFlvClient clientContext,
            IFlvStreamContext streamContext,
            uint timestamp,
            CancellationToken cancellation);

        ValueTask SendCachedMetaDataTagAsync(
            IFlvClient client,
            IFlvStreamContext streamContext,
            uint timestamp,
            CancellationToken cancellation);

        ValueTask SendCachedGroupOfPicturesTagsAsync(
            IFlvClient client,
            IFlvStreamContext streamContext,
            CancellationToken cancellation);
    }
}
