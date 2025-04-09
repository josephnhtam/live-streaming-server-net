using LiveStreamingServerNet.StreamProcessor.Hls.Subtitling;
using LiveStreamingServerNet.StreamProcessor.Hls.Subtitling.Contracts;
using LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling.Contracts
{
    internal interface ISubtitleTranscriberFactory
    {
        ISubtitleTranscriber Create(
            SubtitleTrackOptions options,
            SubtitleTranscriberConfiguration config,
            ITranscriptionStream transcriptionStream,
            ISubtitleCueExtractor subtitleCueExtractor,
            DateTime initialProgramDateTime
        );
    }
}
