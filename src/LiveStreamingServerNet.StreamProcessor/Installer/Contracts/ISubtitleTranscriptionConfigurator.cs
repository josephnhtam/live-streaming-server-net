using LiveStreamingServerNet.StreamProcessor.Hls.Subtitling.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Installer.Contracts
{
    /// <summary>
    /// Interface for configuring subtitle transcription settings.
    /// </summary>
    public interface ISubtitleTranscriptionConfigurator
    {
        /// <summary>
        /// Configure the subtitle cue extractor factory.
        /// </summary>
        /// <param name="configure">Action to configure the subtitle cue extractor factory.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ISubtitleTranscriptionConfigurator WithSubtitleCueExtractorFactory(Func<IServiceProvider, ISubtitleCueExtractorFactory> implementationFactory);
    }
}
