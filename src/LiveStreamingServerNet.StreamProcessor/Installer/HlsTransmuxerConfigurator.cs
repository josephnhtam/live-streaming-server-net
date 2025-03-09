using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.Transcriptions;
using LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.StreamProcessor.Installer
{
    internal record SubtitleTranscriptionStreamFactoryConfiguration(
        SubtitleTrackOptions Options,
        Func<IServiceProvider, ITranscriptionStreamFactory> Factory
    );

    internal class HlsTransmuxerConfigurator : IHlsTransmuxerConfigurator
    {
        private readonly HlsTransmuxerConfiguration _config;
        private readonly List<SubtitleTranscriptionStreamFactoryConfiguration> _subtitleStreamFactoryConfig;

        public IServiceCollection Services { get; }

        public HlsTransmuxerConfigurator(
            IServiceCollection services,
            HlsTransmuxerConfiguration config,
            List<SubtitleTranscriptionStreamFactoryConfiguration> subtitleStreamFactoryConfig)
        {
            Services = services;
            _config = config;
            _subtitleStreamFactoryConfig = subtitleStreamFactoryConfig;
        }

        public IHlsTransmuxerConfigurator Configure(Action<HlsTransmuxerConfiguration> configure)
        {
            configure.Invoke(_config);
            return this;
        }

        public IHlsTransmuxerConfigurator AddSubtitleTranscriptionStreamFactory(
            Func<IServiceProvider, ITranscriptionStreamFactory> factory, SubtitleTrackOptions? options = null)
        {
            _subtitleStreamFactoryConfig.Add(new SubtitleTranscriptionStreamFactoryConfiguration(
                options ?? new SubtitleTrackOptions(), factory));
            return this;
        }
    }
}
