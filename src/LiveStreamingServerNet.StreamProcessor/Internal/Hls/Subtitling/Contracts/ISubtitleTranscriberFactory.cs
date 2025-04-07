using LiveStreamingServerNet.StreamProcessor.Transcriptions;
using LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling.Contracts
{
    internal interface ISubtitleTranscriberFactory
    {
        ISubtitleTranscriber Create(
            SubtitleTrackOptions options,
            SubtitleTranscriberConfiguration config,
            ITranscriptionStream transcriptionStream,
            DateTime initialProgramDateTime
        );
    }
}
