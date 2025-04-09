using LiveStreamingServerNet.StreamProcessor.Hls.Subtitling;
using LiveStreamingServerNet.StreamProcessor.Hls.Subtitling.Contracts;
using LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling
{
    internal record SubtitleTranscriptionConfiguration(
        SubtitleTrackOptions Options, Func<IServiceProvider, ITranscriptionStreamFactory> TranscriptionStreamFactory)
    {
        public Func<IServiceProvider, ISubtitleCueExtractorFactory>? SubtitleCueExtractorFactory { get; init; }
    }
}
