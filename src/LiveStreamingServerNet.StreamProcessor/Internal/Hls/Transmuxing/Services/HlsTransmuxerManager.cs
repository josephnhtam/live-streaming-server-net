using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Services
{
    internal class HlsTransmuxerManager : IHlsTransmuxerManager
    {
        private readonly ConcurrentDictionary<string, IHlsTransmuxer> _transmuxers;

        public HlsTransmuxerManager()
        {
            _transmuxers = new ConcurrentDictionary<string, IHlsTransmuxer>();
        }

        public ValueTask OnReceiveMediaMessage(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp)
        {
            if (_transmuxers.TryGetValue(streamPath, out var transmuxer))
                return transmuxer.AddMediaPacket(mediaType, rentedBuffer, timestamp);

            return ValueTask.CompletedTask;
        }

        public bool RegisterTransmuxer(string streamPath, IHlsTransmuxer transmuxer)
        {
            return _transmuxers.TryAdd(streamPath, transmuxer);
        }

        public void UnregisterTransmuxer(string streamPath)
        {
            _transmuxers.TryRemove(streamPath, out _);
        }
    }
}
