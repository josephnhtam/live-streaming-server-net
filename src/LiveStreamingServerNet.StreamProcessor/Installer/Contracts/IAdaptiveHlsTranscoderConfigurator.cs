using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;

namespace LiveStreamingServerNet.StreamProcessor.Installer.Contracts
{
    public interface IAdaptiveHlsTranscoderConfigurator
    {
        /// <summary>
        /// Configures the default adaptive HLS transcoder settings.
        /// </summary>
        /// <param name="configure"></param>
        /// <returns>The configurator instance for method chaining.</returns>
        IAdaptiveHlsTranscoderConfigurator ConfigureDefault(Action<AdaptiveHlsTranscoderConfiguration> configure);

        /// <summary>
        /// Uses a custom configuration resolver for adaptive HLS transcoding.
        /// </summary>
        /// <param name="resolver">The resolver instance to use.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        IAdaptiveHlsTranscoderConfigurator UseConfigurationResolver(IAdaptiveHlsTranscoderConfigurationResolver resolver);
    }
}
