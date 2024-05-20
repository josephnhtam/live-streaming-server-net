using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Hls.Configurations;
using LiveStreamingServerNet.Transmuxer.Installer.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Hls;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Services;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.Transmuxer.Installer
{
    public static class HlsTransmuxerInstaller
    {
        public static ITransmuxerBuilder AddHlsTransmuxer(this ITransmuxerBuilder transmuxerBuilder, Action<HlsTransmuxerConfiguration>? configure = null)
        {
            var services = transmuxerBuilder.Services;

            var config = new HlsTransmuxerConfiguration();
            configure?.Invoke(config);

            services.AddSingleton<ITransmuxerFactory, HlsTransmuxerFactory>();
            services.TryAddSingleton<IHlsTransmuxerManager, HlsTransmuxerManager>();
            services.TryAddSingleton<IRtmpMediaMessageInterceptor, HlsRtmpMediaMessageScraper>();

            return transmuxerBuilder;
        }
    }
}
