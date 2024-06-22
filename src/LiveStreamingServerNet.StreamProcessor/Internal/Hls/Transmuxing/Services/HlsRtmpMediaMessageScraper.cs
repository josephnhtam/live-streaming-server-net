using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Services
{
    internal class HlsRtmpMediaMessageScraper : IRtmpMediaMessageInterceptor
    {
        private readonly IHlsTransmuxerManager _transmuxerManager;

        public HlsRtmpMediaMessageScraper(IHlsTransmuxerManager transmuxerManager)
        {
            _transmuxerManager = transmuxerManager;
        }

        public ValueTask OnReceiveMediaMessage(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable)
        {
            return _transmuxerManager.OnReceiveMediaMessage(streamPath, mediaType, rentedBuffer, timestamp);
        }
    }
}
