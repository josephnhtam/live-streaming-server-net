using LiveStreamingServerNet.StreamProcessor.Hls.Subtitling;
using LiveStreamingServerNet.StreamProcessor.Hls.Subtitling.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling;
using LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Installer
{
    internal class SubtitleTranscriptionConfigurator : ISubtitleTranscriptionConfigurator
    {
        private SubtitleTranscriptionConfiguration _config;

        public SubtitleTranscriptionConfigurator(SubtitleTrackOptions options, Func<IServiceProvider, ITranscriptionStreamFactory> factory)
        {
            _config = new SubtitleTranscriptionConfiguration(options, factory);
        }

        public ISubtitleTranscriptionConfigurator WithSubtitleCueExtractorFactory(
            Func<IServiceProvider, ISubtitleCueExtractorFactory> implementationFactory)
        {
            _config = _config with { SubtitleCueExtractorFactory = implementationFactory };
            return this;
        }

        public SubtitleTranscriptionConfiguration Build()
        {
            return _config;
        }
    }
}
