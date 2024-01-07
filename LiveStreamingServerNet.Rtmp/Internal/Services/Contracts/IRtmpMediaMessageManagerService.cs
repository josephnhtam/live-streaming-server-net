using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpMediaMessageManagerService : IAsyncDisposable
    {
        void EnqueueVideoMessage(
            IRtmpClientPeerContext subscriber,
            uint timestamp,
            uint streamId,
            bool isSkippable,
            Action<INetBuffer> payloadWriter);

        void EnqueueVideoMessage(
            IList<IRtmpClientPeerContext> subscribers,
            uint timestamp,
            uint streamId,
            bool isSkippable,
            Action<INetBuffer> payloadWriter);

        void EnqueueAudioMessage(
            IRtmpClientPeerContext subscriber,
            uint timestamp,
            uint streamId,
            bool isSkippable,
            Action<INetBuffer> payloadWriter);

        void EnqueueAudioMessage(
            IList<IRtmpClientPeerContext> subscribers,
            uint timestamp,
            uint streamId,
            bool isSkippable,
            Action<INetBuffer> payloadWriter);

        void SendCachedHeaderMessages(
            IRtmpClientPeerContext peerContext,
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            uint streamId);

        void SendCachedStreamMetaDataMessage(
            IRtmpClientPeerContext peerContext,
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            uint streamId);

        void SendCachedStreamMetaDataMessage(
            IList<IRtmpClientPeerContext> peerContexts,
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            uint streamId);

        void SendCachedGroupOfPictures(
            IRtmpClientPeerContext peerContext,
            IRtmpPublishStreamContext publishStreamContext,
            uint streamId);

        void RegisterClientPeer(IRtmpClientPeerContext peerContext);
        void UnregisterClientPeer(IRtmpClientPeerContext peerContext);
    }
}