using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing
{
    internal class HlsSubtitledMediaPacketInterceptor : IHlsMediaPacketInterceptor
    {
        private readonly IReadOnlyList<ISubtitleTranscriber> _subtitleTranscribers;

        public HlsSubtitledMediaPacketInterceptor(IReadOnlyList<ISubtitleTranscriber> subtitleTranscribers)
        {
            _subtitleTranscribers = subtitleTranscribers;
        }

        public async ValueTask InterceptMediaPacketAsync(MediaType mediaType, IRentedBuffer buffer, uint timestamp)
        {
            if (mediaType != MediaType.Audio)
                return;

            foreach (var transcriber in _subtitleTranscribers)
            {
                await transcriber.EnqueueAudioBufferAsync(buffer, timestamp).ConfigureAwait(false);
            }
        }
    }
}
