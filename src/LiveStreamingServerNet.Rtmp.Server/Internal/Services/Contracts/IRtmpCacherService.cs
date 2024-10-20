using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts
{
    internal interface IRtmpCacherService : IAsyncDisposable
    {
        ValueTask CacheStreamMetaDataAsync(
            IRtmpPublishStreamContext publishStreamContext,
            IReadOnlyDictionary<string, object> metaData);

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
            IRtmpSubscribeStreamContext subscribeStreamContext,
            IRtmpPublishStreamContext publishStreamContext);

        void SendCachedStreamMetaDataMessage(
            IRtmpSubscribeStreamContext subscribeStreamContext,
            IRtmpPublishStreamContext publishStreamContext);

        void SendCachedStreamMetaDataMessage(
            IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts,
            IRtmpPublishStreamContext publishStreamContext);

        void SendCachedGroupOfPictures(
            IRtmpSubscribeStreamContext subscribeStreamContext,
            IRtmpPublishStreamContext publishStreamContext);
    }
}