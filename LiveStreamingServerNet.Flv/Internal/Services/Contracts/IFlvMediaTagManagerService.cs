using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services.Contracts
{
    internal interface IFlvMediaTagManagerService
    {
        Task EnqueueMediaTagAsync(
            IFlvStreamContext streamContext,
            IList<IFlvClient> subscribers,
            MediaType mediaType,
            uint timestamp,
            bool isSkippable,
            IRentedBuffer rentedBuffer);

        Task CacheSequenceHeaderAsync(
            IFlvStreamContext streamContext,
            MediaType mediaType,
            byte[] sequenceHeader);

        Task CachePictureAsync(
            IFlvStreamContext streamContext,
            MediaType mediaType,
            IRentedBuffer rentedBuffer,
            uint timestamp);

        Task ClearGroupOfPicturesCacheAsync(IFlvStreamContext streamContext);

        Task SendCachedHeaderTagsAsync(
            IFlvClient clientContext,
            IFlvStreamContext streamContext,
            uint timestamp,
            CancellationToken cancellation);

        Task SendCachedMetaDataTagAsync(
            IFlvClient client,
            IFlvStreamContext streamContext,
            uint timestamp,
            CancellationToken cancellation);

        Task SendCachedGroupOfPicturesTagsAsync(
            IFlvClient client,
            IFlvStreamContext streamContext,
            CancellationToken cancellation);

        void RegisterClient(IFlvClient client);
        void UnregisterClient(IFlvClient client);
    }
}
