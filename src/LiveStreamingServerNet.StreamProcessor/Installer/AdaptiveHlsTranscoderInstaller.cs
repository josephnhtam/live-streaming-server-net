using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.AdaptiveTranscoding;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.Installer
{
    /// <summary>
    /// Provides extension methods for installing and configuring adaptive HLS transcoding services.
    /// </summary>
    public static class AdaptiveHlsTranscoderInstaller
    {
        /// <summary>
        /// Adds adaptive HLS transcoding to the stream processing.
        /// </summary>
        /// <param name="builder">The stream processing builder to add services to.</param>
        /// <returns>The stream processing builder for method chaining.</returns>
        public static IStreamProcessingBuilder AddAdaptiveHlsTranscoder(this IStreamProcessingBuilder builder)
            => AddAdaptiveHlsTranscoder(builder, null);

        /// <summary>
        /// Adds adaptive HLS transcoding to the stream processing.
        /// </summary>
        /// <param name="builder">The stream processing builder to add services to.</param>
        /// <param name="configure">Optional action to configure the adaptive HLS transcoder.</param>
        /// <returns>The stream processing builder for method chaining.</returns>
        public static IStreamProcessingBuilder AddAdaptiveHlsTranscoder(
            this IStreamProcessingBuilder builder, Action<AdaptiveHlsTranscoderConfiguration>? configure)
        {
            var services = builder.Services;

            var config = new AdaptiveHlsTranscoderConfiguration();
            configure?.Invoke(config);

            services.TryAddSingleton<IHlsPathRegistry, HlsPathRegistry>();
            services.TryAddSingleton<IHlsPathMapper>(svc => svc.GetRequiredService<IHlsPathRegistry>());

            services.TryAddSingleton<IHlsCleanupManager, HlsCleanupManager>();

            services.AddSingleton<IStreamProcessorFactory>(svc =>
                new AdaptiveHlsTranscoderFactory(
                    svc,
                    svc.GetRequiredService<IHlsCleanupManager>(),
                    svc.GetRequiredService<IHlsPathRegistry>(),
                    config,
                    svc.GetRequiredService<ILogger<AdaptiveHlsTranscoder>>()
                )
            );

            return builder;
        }
    }
}
