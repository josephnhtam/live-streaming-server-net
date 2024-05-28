using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Contracts
{
    internal interface IHlsTransmuxer : IStreamProcessor
    {
        ValueTask AddMediaPacket(MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp);
    }
}
