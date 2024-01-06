using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.Rtmp.Services.Contracts
{
    public interface IRtmpMediaMessageManagerService : IAsyncDisposable
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

        void SendCachedStreamMetaData(
            IRtmpClientPeerContext peerContext,
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            uint streamId);

        void SendCachedStreamMetaData(
            IList<IRtmpClientPeerContext> peerContexts,
            IRtmpPublishStreamContext publishStreamContext,
            uint timestamp,
            uint streamId);

        void RegisterClientPeer(IRtmpClientPeerContext peerContext);
        void UnregisterClientPeer(IRtmpClientPeerContext peerContext);
    }
}