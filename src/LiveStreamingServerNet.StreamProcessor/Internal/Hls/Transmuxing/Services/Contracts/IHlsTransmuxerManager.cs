using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Services.Contracts
{
    internal interface IHlsTransmuxerManager
    {
        bool RegisterTransmuxer(string streamPath, IHlsTransmuxer transmuxer);
        void UnregisterTransmuxer(string streamPath);

        ValueTask OnReceiveMediaMessage(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp);
    }
}
