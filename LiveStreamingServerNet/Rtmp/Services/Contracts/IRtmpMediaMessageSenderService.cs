using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.Rtmp.Services.Contracts
{
    public interface IRtmpMediaMessageSenderService
    {
        void SendVideoMessage(
            IRtmpClientPeerContext subscriber,
            IRtmpChunkStreamContext chunkStreamContext,
            Action<INetBuffer> payloadWriter,
            Action? callback = null);

        void SendVideoMessage(
            IList<IRtmpClientPeerContext> subscribers,
            IRtmpChunkStreamContext chunkStreamContext,
            Action<INetBuffer> payloadWriter);

        Task SendVideoMessageAsync(
            IRtmpClientPeerContext subscriber,
            IRtmpChunkStreamContext chunkStreamContext,
            Action<INetBuffer> payloadWriter);

        void SendAudioMessage(
            IRtmpClientPeerContext subscriber,
            IRtmpChunkStreamContext chunkStreamContext,
            Action<INetBuffer> payloadWriter,
            Action? callback = null);

        void SendAudioMessage(
            IList<IRtmpClientPeerContext> subscribers,
            IRtmpChunkStreamContext chunkStreamContext,
            Action<INetBuffer> payloadWriter);

        Task SendAudioMessageAsync(
            IRtmpClientPeerContext subscriber,
            IRtmpChunkStreamContext chunkStreamContext,
            Action<INetBuffer> payloadWriter);
    }
}