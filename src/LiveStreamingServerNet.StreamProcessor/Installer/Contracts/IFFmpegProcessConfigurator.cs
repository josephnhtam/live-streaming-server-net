using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.FFmpeg.Configurations;

namespace LiveStreamingServerNet.StreamProcessor.Installer.Contracts
{
    public interface IFFmpegProcessConfigurator
    {
        /// <summary>
        /// Configures the default FFmpeg process settings.
        /// </summary>
        /// <param name="configure"></param>
        /// <returns>The configurator instance for method chaining.</returns>
        IFFmpegProcessConfigurator ConfigureDefault(Action<FFmpegProcessConfiguration> configure);

        /// <summary>
        /// Uses a custom configuration resolver for FFmpeg processing.
        /// </summary>
        /// <param name="resolver">The resolver instance to use.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        IFFmpegProcessConfigurator UseConfigurationResolver(IFFmpegProcessConfigurationResolver resolver);
    }
}
