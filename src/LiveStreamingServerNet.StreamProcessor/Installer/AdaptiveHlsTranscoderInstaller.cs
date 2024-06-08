using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.AdaptiveTranscoding;
using Microsoft.Extensions.DependencyInjection;
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

            services.AddSingleton<IStreamProcessorFactory>(svc =>
                new AdaptiveHlsTranscoderFactory(
                    svc,
                    config,
                    svc.GetRequiredService<ILogger<AdaptiveHlsTranscoder>>()
                )
            );

            return builder;
        }
    }
}
