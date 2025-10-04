using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Hls.Subtitling;
using LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.StreamProcessor.Installer.Contracts
{
    /// <summary>
    /// Defines a configuration interface for setting up HLS transmuxers.
    /// </summary>
    public interface IHlsTransmuxerConfigurator
    {
        /// <summary>
        /// Gets the service collection for dependency injection configuration.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Configures the HLS transmuxer.
        /// </summary>
        /// <param name="configure">Action to configure the HLS transmuxer.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        IHlsTransmuxerConfigurator Configure(Action<HlsTransmuxerConfiguration> configure);

        /// <summary>
        /// Adds a subtitle transcription.
        /// </summary>
        /// <param name="options">Options for the subtitle track.</param>
        /// <param name="transcriptionStreamFactory">Factory method to create the transcription stream.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        IHlsTransmuxerConfigurator AddSubtitleTranscription(
            SubtitleTrackOptions options,
            Func<IServiceProvider, ITranscriptionStreamFactory> transcriptionStreamFactory);

        /// <summary>
        /// Adds a subtitle transcription.
        /// </summary>
        /// <param name="options">Options for the subtitle track.</param>
        /// <param name="factory">Factory method to create the transcription stream.</param>
        /// <param name="configure">Action to configure the subtitle transcription.</param>
        /// <returns>The configurator instancefor method chaining.</returns>
        IHlsTransmuxerConfigurator AddSubtitleTranscription(
            SubtitleTrackOptions options,
            Func<IServiceProvider, ITranscriptionStreamFactory> transcriptionStreamFactory,
            Action<ISubtitleTranscriptionConfigurator>? configure);
    }
}
