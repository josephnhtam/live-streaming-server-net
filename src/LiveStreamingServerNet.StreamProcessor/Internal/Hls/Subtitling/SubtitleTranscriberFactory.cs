using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling.Contracts;
using LiveStreamingServerNet.StreamProcessor.Transcriptions;
using LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling
{
    internal class SubtitleTranscriberFactory : ISubtitleTranscriberFactory
    {
        private readonly ILogger<SubtitleTranscriber> _logger;

        public SubtitleTranscriberFactory(ILogger<SubtitleTranscriber> logger)
        {
            _logger = logger;
        }

        public ISubtitleTranscriber Create(
            SubtitleTrackOptions options,
            SubtitleTranscriberConfiguration config,
            ITranscriptionStream transcriptionStream,
            DateTime initialProgramDateTime)
        {
            return new SubtitleTranscriber(
                options,
                config,
                transcriptionStream,
                initialProgramDateTime,
                _logger
            );
        }
    }
}
