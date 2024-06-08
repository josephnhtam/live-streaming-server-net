using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.AdaptiveTranscoding;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.Installer
{
    public static class AdaptiveHlsTranscoderInstaller
    {
        public static IStreamProcessingBuilder AddAdaptiveHlsTranscoder(
            this IStreamProcessingBuilder builder, Action<AdaptiveHlsTranscoderConfiguration>? configure = null)
        {
            var services = builder.Services;

            var config = new AdaptiveHlsTranscoderConfiguration();
            configure?.Invoke(config);

            services.TryAddSingleton<IHlsCleanupManager, HlsCleanupManager>();

            services.AddSingleton<IStreamProcessorFactory>(svc =>
                new AdaptiveHlsTranscoderFactory(
                    svc,
                    svc.GetRequiredService<IHlsCleanupManager>(),
                    config,
                    svc.GetRequiredService<ILogger<AdaptiveHlsTranscoder>>()
                )
            );

            return builder;
        }
    }
}
