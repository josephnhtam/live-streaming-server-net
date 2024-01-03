using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.Rtmp.Services.Contracts
{
    public interface IRtmpMediaMessageManagerService : IAsyncDisposable
    {
        void EnqueueVideoMessage(
            IRtmpClientPeerContext subscriber,
            IRtmpChunkStreamContext chunkStreamContext,
            bool isSkippable,
            Action<INetBuffer> payloadWriter);

        void EnqueueVideoMessage(
            IList<IRtmpClientPeerContext> subscribers,
            IRtmpChunkStreamContext chunkStreamContext,
            bool isSkippable,
            Action<INetBuffer> payloadWriter);

        void EnqueueAudioMessage(
            IRtmpClientPeerContext subscriber,
            IRtmpChunkStreamContext chunkStreamContext,
            bool isSkippable,
            Action<INetBuffer> payloadWriter);

        void EnqueueAudioMessage(
            IList<IRtmpClientPeerContext> subscribers,
            IRtmpChunkStreamContext chunkStreamContext,
            bool isSkippable,
            Action<INetBuffer> payloadWriter);

        void RegisterClientPeer(IRtmpClientPeerContext peerContext);
        void UnregisterClientPeer(IRtmpClientPeerContext peerContext);
    }
}