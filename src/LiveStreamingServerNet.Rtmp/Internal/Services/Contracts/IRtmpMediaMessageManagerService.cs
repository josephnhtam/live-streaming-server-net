using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpMediaMessageManagerService : IAsyncDisposable
    {
        ValueTask EnqueueMediaMessageAsync(
            IRtmpPublishStreamContext publishStreamContext,
            IList<IRtmpClientContext> subscribers,
            MediaType mediaType,
            uint timestamp,
            bool isSkippable,
            Action<INetBuffer> payloadWriter);

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
            uint streamId);

        void SendCachedStreamMetaDataMessage(
            IRtmpClientContext clientContext,
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            uint streamId);

        void SendCachedStreamMetaDataMessage(
            IList<IRtmpClientContext> clientContexts,
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            uint streamId);

        void SendCachedGroupOfPictures(
            IRtmpClientContext clientContext,
            IRtmpPublishStreamContext publishStreamContext,
            uint streamId);

        void RegisterClient(IRtmpClientContext clientContext);
        void UnregisterClient(IRtmpClientContext clientContext);
    }
}