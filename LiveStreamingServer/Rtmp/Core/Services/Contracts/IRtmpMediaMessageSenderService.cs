using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpHeaders;

namespace LiveStreamingServer.Rtmp.Core.Services.Contracts
{
    public interface IRtmpMediaMessageSenderService
    {
        void SendVideoMessage<TRtmpChunkMessageHeader>(
            IRtmpClientPeerContext subscriber,
            Action<INetBuffer> payloadWriter,
            Action? callback = null);

        void SendVideoMessage<TRtmpChunkMessageHeader>(
            IList<IRtmpClientPeerContext> subscribers,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<INetBuffer> payloadWriter);

        Task SendVideoMessageAsync<TRtmpChunkMessageHeader>(
            IRtmpClientPeerContext subscriber,
            Action<INetBuffer> payloadWriter);

        void SendAudioMessage<TRtmpChunkMessageHeader>(
            IRtmpClientPeerContext subscriber,
            Action<INetBuffer> payloadWriter,
            Action? callback = null);

        void SendAudioMessage<TRtmpChunkMessageHeader>(
            IList<IRtmpClientPeerContext> subscribers,
            Action<INetBuffer> payloadWriter);

        Task SendAudioMessageAsync<TRtmpChunkMessageHeader>(
            IRtmpClientPeerContext subscriber,
            Action<INetBuffer> payloadWriter);
    }
}