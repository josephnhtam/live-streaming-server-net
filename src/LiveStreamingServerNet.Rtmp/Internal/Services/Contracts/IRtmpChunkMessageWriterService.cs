using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpChunkMessageWriterService
    {
        void Write<TRtmpChunkMessageHeader>(
            INetBuffer targetBuffer,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            INetBuffer payloadBuffer,
            uint outChunkSize) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader;
    }
}
