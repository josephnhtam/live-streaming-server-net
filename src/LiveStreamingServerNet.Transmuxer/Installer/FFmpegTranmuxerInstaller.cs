using LiveStreamingServerNet.Transmuxer.Configurations;
using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.Transmuxer.Installer
{
    public static class FFmpegTranmuxerInstaller
    {
        public static ITransmuxerBuilder UseFFmpeg(this ITransmuxerBuilder transmuxerBuilder, Action<FFmpegTransmuxerFactoryConfiguration>? configure = null)
        {
            transmuxerBuilder.Services.TryAddSingleton<ITransmuxerFactory, FFmpegTransmuxerFactory>();

            if (configure != null)
                transmuxerBuilder.Services.Configure(configure);

            return transmuxerBuilder;
        }
    }
}
