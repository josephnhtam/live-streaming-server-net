using LiveStreamingServerNet.Transmuxer.Configurations;
using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Transmuxer.Installer
{
    public static class FFmpegTranmuxerInstaller
    {
        public static ITransmuxerBuilder AddFFmpeg(this ITransmuxerBuilder transmuxerBuilder, Action<FFmpegTransmuxerFactoryConfiguration>? configure = null)
        {
            var config = new FFmpegTransmuxerFactoryConfiguration();
            configure?.Invoke(config);

            transmuxerBuilder.Services.AddSingleton<ITransmuxerFactory>(_ => new FFmpegTransmuxerFactory(config));

            return transmuxerBuilder;
        }
    }
}
