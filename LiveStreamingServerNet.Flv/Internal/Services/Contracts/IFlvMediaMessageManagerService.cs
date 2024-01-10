using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp;

namespace LiveStreamingServerNet.Flv.Internal.Services.Contracts
{
    internal interface IFlvMediaMessageManagerService
    {
        Task EnqueueMediaMessageAsync(
            IFlvStreamContext streamContext,
            IList<IFlvClient> subscribers,
            MediaType mediaType,
            uint timestamp,
            bool isSkippable,
            Action<INetBuffer> payloadWriter);

        Task CacheSequenceHeaderAsync(
            IFlvStreamContext streamContext,
            MediaType mediaType,
            INetBuffer payloadBuffer);

        Task CachePictureAsync(
            IFlvStreamContext streamContext,
            MediaType mediaType,
            INetBuffer payloadBuffer,
            uint timestamp);

        Task ClearGroupOfPicturesCacheAsync(IFlvStreamContext streamContext);

        void SendCachedHeaderMessages(
            IFlvClient client,
            IFlvStreamContext streamContext,
            uint timestamp,
            uint streamId);

        void SendCachedGroupOfPictures(
            IFlvClient client,
            IFlvStreamContext streamContext,
            uint streamId);

        void RegisterClient(IFlvClient client);
        void UnregisterClient(IFlvClient client);
    }
}
