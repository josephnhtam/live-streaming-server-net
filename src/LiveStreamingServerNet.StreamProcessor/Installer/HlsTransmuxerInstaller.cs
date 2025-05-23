﻿using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output.Writers.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Services;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Transmuxing.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.StreamProcessor.Installer
{
    /// <summary>
    /// Provides extension methods for installing and configuring HLS transmuxing services.
    /// </summary>
    public static class HlsTransmuxerInstaller
    {
        /// <summary>
        /// Adds HLS transmuxing to the stream processing.
        /// </summary>
        /// <param name="builder">The stream processing builder to add services to.</param>
        /// <returns>The stream processing builder for method chaining.</returns>
        public static IStreamProcessingBuilder AddHlsTransmuxer(this IStreamProcessingBuilder builder)
            => AddHlsTransmuxer(builder, null);

        /// <summary>
        /// Adds HLS transmuxing to the stream processing.
        /// </summary>
        /// <param name="builder">The stream processing builder to add services to.</param>
        /// <param name="configure">Optional action to configure the HLS transmuxer.</param>
        /// <returns>The stream processing builder for method chaining.</returns>
        public static IStreamProcessingBuilder AddHlsTransmuxer(this IStreamProcessingBuilder builder, Action<IHlsTransmuxerConfigurator>? configure)
        {
            var services = builder.Services;

            var config = new HlsTransmuxerConfiguration();
            var subtitleTranscriptionConfigs = new List<SubtitleTranscriptionConfiguration>();

            var configurator = new HlsTransmuxerConfigurator(services, config, subtitleTranscriptionConfigs);
            configure?.Invoke(configurator);

            services.TryAddSingleton<IHlsPathRegistry, HlsPathRegistry>();
            services.TryAddSingleton<IHlsPathMapper>(svc => svc.GetRequiredService<IHlsPathRegistry>());

            services.TryAddSingleton<IMasterManifestWriter, MasterManifestWriter>();
            services.TryAddSingleton<IMediaManifestWriter, MediaManifestWriter>();
            services.TryAddSingleton<IHlsTransmuxerManager, HlsTransmuxerManager>();
            services.TryAddSingleton<IHlsCleanupManager, HlsCleanupManager>();
            services.TryAddSingleton<ISubtitleTranscriberFactory, SubtitleTranscriberFactory>();

            services.AddSingleton<IRtmpMediaMessageInterceptor, HlsRtmpMediaMessageScraper>();

            services.AddSingleton<IStreamProcessorFactory>(svc =>
            {
                if (subtitleTranscriptionConfigs.Any())
                {
                    return new HlsSubtitledTransmuxerFactory(svc, subtitleTranscriptionConfigs, config);
                }

                return new HlsTransmuxerFactory(svc, config);
            });

            return builder;
        }
    }
}
