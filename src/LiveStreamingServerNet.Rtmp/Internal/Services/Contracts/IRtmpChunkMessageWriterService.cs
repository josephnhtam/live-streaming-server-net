using LiveStreamingServerNet.Rtmp.Internal.RtmpHeaders;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Services.Contracts
{
    internal interface IRtmpChunkMessageWriterService
    {
        void Write<TRtmpChunkMessageHeader>(
            IDataBuffer targetBuffer,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            IDataBuffer payloadBuffer,
            uint outChunkSize) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader;
    }
}
