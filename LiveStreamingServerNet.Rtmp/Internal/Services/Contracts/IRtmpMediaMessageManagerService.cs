using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpMediaMessageManagerService : IAsyncDisposable
    {
        void EnqueueVideoMessage(
            IRtmpClientContext subscriber,
            uint timestamp,
            uint streamId,
            bool isSkippable,
            Action<INetBuffer> payloadWriter);

        void EnqueueVideoMessage(
            IList<IRtmpClientContext> subscribers,
            uint timestamp,
            uint streamId,
            bool isSkippable,
            Action<INetBuffer> payloadWriter);

        void EnqueueAudioMessage(
            IRtmpClientContext subscriber,
            uint timestamp,
            uint streamId,
            bool isSkippable,
            Action<INetBuffer> payloadWriter);

        void EnqueueAudioMessage(
            IList<IRtmpClientContext> subscribers,
            uint timestamp,
            uint streamId,
            bool isSkippable,
            Action<INetBuffer> payloadWriter);

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