using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpHeaders;

namespace LiveStreamingServerNet.Rtmp.Services.Contracts
{
    internal interface IRtmpChunkMessageSenderService
    {
        void Send<TRtmpChunkMessageHeader>(
            IRtmpClientPeerContext peerContext,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<INetBuffer> payloadWriter,
            Action? callback = null) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader;

        void Send<TRtmpChunkMessageHeader>(
            IList<IRtmpClientPeerContext> peerContexts,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<INetBuffer> payloadWriter) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader;

        Task SendAsync<TRtmpChunkMessageHeader>(
            IRtmpClientPeerContext peerContext,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<INetBuffer> payloadWriter)
            where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader;
    }
}