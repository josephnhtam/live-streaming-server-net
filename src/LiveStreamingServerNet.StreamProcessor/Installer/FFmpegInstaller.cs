using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.FFmpeg.Configurations;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.FFmpeg;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.Installer
{
    /// <summary>
    /// Provides extension methods for installing and configuring FFmpeg processing services.
    /// </summary>
    public static class FFmpegInstaller
    {
        /// <summary>
        /// Adds FFmpeg process to the stream processing.
        /// </summary>
        /// <param name="builder">The stream processing builder to add services to.</param>
        /// <returns>The stream processing builder for method chaining.</returns>
        public static IStreamProcessingBuilder AddFFmpeg(this IStreamProcessingBuilder builder)
            => AddFFmpeg(builder, null);

        /// <summary>
        /// Adds FFmpeg process to the stream processing.
        /// </summary>
        /// <param name="builder">The stream processing builder to add services to.</param>
        /// <param name="configure">Optional action to configure the FFmpeg process.</param>
        /// <returns>The stream processing builder for method chaining.</returns>
        public static IStreamProcessingBuilder AddFFmpeg(this IStreamProcessingBuilder builder, Action<FFmpegProcessConfiguration>? configure)
        {
            var config = new FFmpegProcessConfiguration();
            configure?.Invoke(config);

            builder.Services.AddSingleton<IStreamProcessorFactory>(svc =>
                new FFmpegProcessFactory(svc, config, svc.GetRequiredService<ILogger<FFmpegProcess>>()));

            return builder;
        }
    }
}
