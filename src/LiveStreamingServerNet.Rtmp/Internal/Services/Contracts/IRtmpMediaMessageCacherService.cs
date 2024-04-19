using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpMediaMessageCacherService : IAsyncDisposable
    {
        ValueTask CacheSequenceHeaderAsync(
            IRtmpPublishStreamContext publishStreamContext,
            MediaType mediaType,
            INetBuffer payloadBuffer);

        ValueTask CachePictureAsync(
            IRtmpPublishStreamContext publishStreamContext,
            MediaType mediaType,
            INetBuffer payloadBuffer,
            uint timestamp);

        ValueTask ClearGroupOfPicturesCacheAsync(IRtmpPublishStreamContext publishStreamContext);

        void SendCachedHeaderMessages(
            IRtmpClientContext clientContext,
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            uint messageStreamId);

        void SendCachedStreamMetaDataMessage(
            IRtmpClientContext clientContext,
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            uint messageStreamId);

        void SendCachedStreamMetaDataMessage(
            IReadOnlyList<IRtmpClientContext> clientContexts,
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            uint messageStreamId);

        void SendCachedGroupOfPictures(
            IRtmpClientContext clientContext,
            IRtmpPublishStreamContext publishStreamContext,
            uint messageStreamId);
    }
}