using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpChunkMessageSenderService
    {
        void Send<TRtmpChunkMessageHeader>(
            IRtmpClientContext clientContext,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<INetBuffer> payloadWriter,
            Action<bool>? callback = null) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader;

        void Send<TRtmpChunkMessageHeader>(
            IReadOnlyList<IRtmpClientContext> clientContexts,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<INetBuffer> payloadWriter) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader;

        ValueTask SendAsync<TRtmpChunkMessageHeader>(
            IRtmpClientContext clientContext,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<INetBuffer> payloadWriter)
            where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader;
    }
}