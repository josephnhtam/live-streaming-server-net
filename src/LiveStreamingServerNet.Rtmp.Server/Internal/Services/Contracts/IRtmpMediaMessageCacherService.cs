using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts
{
    internal interface IRtmpMediaMessageCacherService : IAsyncDisposable
    {
        ValueTask CacheSequenceHeaderAsync(
            IRtmpPublishStreamContext publishStreamContext,
            MediaType mediaType,
            IDataBuffer payloadBuffer);

        ValueTask CachePictureAsync(
            IRtmpPublishStreamContext publishStreamContext,
            MediaType mediaType,
            IDataBuffer payloadBuffer,
            uint timestamp);

        ValueTask ClearGroupOfPicturesCacheAsync(IRtmpPublishStreamContext publishStreamContext);

        void SendCachedHeaderMessages(
            IRtmpClientSessionContext clientContext,
            IRtmpPublishStreamContext publishStreamContext,
            uint messageStreamId);

        void SendCachedStreamMetaDataMessage(
            IRtmpClientSessionContext clientContext,
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            uint messageStreamId);

        void SendCachedStreamMetaDataMessage(
            IReadOnlyList<IRtmpClientSessionContext> clientContexts,
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            uint messageStreamId);

        void SendCachedGroupOfPictures(
            IRtmpClientSessionContext clientContext,
            IRtmpPublishStreamContext publishStreamContext,
            uint messageStreamId);
    }
}