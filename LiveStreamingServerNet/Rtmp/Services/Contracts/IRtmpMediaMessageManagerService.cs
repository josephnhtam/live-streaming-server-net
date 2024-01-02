using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.Rtmp.Services.Contracts
{
    public interface IRtmpMediaMessageManagerService
    {
        void SendVideoMessage(
            IRtmpClientPeerContext subscriber,
            IRtmpChunkStreamContext chunkStreamContext,
            bool isSkippable,
            Action<INetBuffer> payloadWriter);

        void SendVideoMessage(
            IList<IRtmpClientPeerContext> subscribers,
            IRtmpChunkStreamContext chunkStreamContext,
            bool isSkippable,
            Action<INetBuffer> payloadWriter);

        void SendAudioMessage(
            IRtmpClientPeerContext subscriber,
            IRtmpChunkStreamContext chunkStreamContext,
            bool isSkippable,
            Action<INetBuffer> payloadWriter);

        void SendAudioMessage(
            IList<IRtmpClientPeerContext> subscribers,
            IRtmpChunkStreamContext chunkStreamContext,
            bool isSkippable,
            Action<INetBuffer> payloadWriter);

        void RegisterClientPeer(IRtmpClientPeerContext peerContext);
        void UnregisterClientPeer(IRtmpClientPeerContext peerContext);
    }
}