using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Transcriptions;
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
        /// <returns>The configurator instance for method chaining.</returns
        IHlsTransmuxerConfigurator Configure(Action<HlsTransmuxerConfiguration> configure);

        /// <summary>
        /// Registers a transcription stream factory for a subtitle track.
        /// </summary>
        /// <param name="factory">Factory method to create the transcription stream.</param>
        /// <param name="options">Optional options for the subtitle track.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        IHlsTransmuxerConfigurator AddSubtitleTranscriptionStreamFactory(
            Func<IServiceProvider, ITranscriptionStreamFactory> factory, SubtitleTrackOptions? options = null);
    }
}
