using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpChunkMessageSenderService
    {
        void Send<TRtmpChunkMessageHeader>(
            IRtmpClientContext clientContext,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<INetBuffer> payloadWriter,
            Action? callback = null) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader;

        void Send<TRtmpChunkMessageHeader>(
            IList<IRtmpClientContext> clientContexts,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<INetBuffer> payloadWriter) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader;

        Task SendAsync<TRtmpChunkMessageHeader>(
            IRtmpClientContext clientContext,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<INetBuffer> payloadWriter)
            where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader;
    }
}