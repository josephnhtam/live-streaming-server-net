using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.FFmpeg.Configurations;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.FFmpeg;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.StreamProcessor.Installer
{
    public static class FFmpegInstaller
    {
        public static IStreamProcessingBuilder AddFFmpeg(this IStreamProcessingBuilder builder, Action<FFmpegProcessConfiguration>? configure = null)
        {
            var config = new FFmpegProcessConfiguration();
            configure?.Invoke(config);

            builder.Services.AddSingleton<IStreamProcessorFactory>(svc => new FFmpegProcessFactory(svc, config));

            return builder;
        }
    }
}
