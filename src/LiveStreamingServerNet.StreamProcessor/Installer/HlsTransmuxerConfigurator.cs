using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Hls.Subtitling;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling;
using LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.StreamProcessor.Installer
{
    internal class HlsTransmuxerConfigurator : IHlsTransmuxerConfigurator
    {
        private readonly HlsTransmuxerConfiguration _config;
        private readonly List<SubtitleTranscriptionConfiguration> _subtitleTranscriptionConfigs;

        public IServiceCollection Services { get; }

        public HlsTransmuxerConfigurator(
            IServiceCollection services,
            HlsTransmuxerConfiguration config,
            List<SubtitleTranscriptionConfiguration> subtitleTranscriptionConfigs)
        {
            Services = services;
            _config = config;
            _subtitleTranscriptionConfigs = subtitleTranscriptionConfigs;
        }

        public IHlsTransmuxerConfigurator Configure(Action<HlsTransmuxerConfiguration> configure)
        {
            configure.Invoke(_config);
            return this;
        }

        public IHlsTransmuxerConfigurator AddSubtitleTranscription(
            SubtitleTrackOptions options,
            Func<IServiceProvider, ITranscriptionStreamFactory> factory)
        {
            return AddSubtitleTranscription(options, factory, null);
        }

        public IHlsTransmuxerConfigurator AddSubtitleTranscription(
            SubtitleTrackOptions options,
            Func<IServiceProvider, ITranscriptionStreamFactory> factory,
            Action<ISubtitleTranscriptionConfigurator>? configure)
        {
            var configurator = new SubtitleTranscriptionConfigurator(options, factory);
            configure?.Invoke(configurator);

            var config = configurator.Build();
            _subtitleTranscriptionConfigs.Add(config);

            return this;
        }
    }
}
