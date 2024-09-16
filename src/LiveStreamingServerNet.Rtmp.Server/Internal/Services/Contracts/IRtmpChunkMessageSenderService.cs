using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts
{
    internal interface IRtmpChunkMessageSenderService
    {
        void Send<TRtmpChunkMessageHeader>(
            IRtmpClientSessionContext clientContext,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<IDataBuffer> payloadWriter,
            Action<bool>? callback = null) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader;

        void Send<TRtmpChunkMessageHeader>(
            IReadOnlyList<IRtmpClientSessionContext> clientContexts,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<IDataBuffer> payloadWriter) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader;

        ValueTask SendAsync<TRtmpChunkMessageHeader>(
            IRtmpClientSessionContext clientContext,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<IDataBuffer> payloadWriter)
            where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader;
    }
}