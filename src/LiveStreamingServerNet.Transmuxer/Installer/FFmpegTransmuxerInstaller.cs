using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.FFmpeg.Configurations;
using LiveStreamingServerNet.Transmuxer.Installer.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.FFmpeg;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Transmuxer.Installer
{
    public static class FFmpegTransmuxerInstaller
    {
        public static ITransmuxerBuilder AddFFmpeg(this ITransmuxerBuilder transmuxerBuilder, Action<FFmpegTransmuxerConfiguration>? configure = null)
        {
            var config = new FFmpegTransmuxerConfiguration();
            configure?.Invoke(config);

            transmuxerBuilder.Services.AddSingleton<ITransmuxerFactory>(_ => new FFmpegTransmuxerFactory(config));

            return transmuxerBuilder;
        }
    }
}
