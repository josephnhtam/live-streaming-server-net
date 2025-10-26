using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.FFmpeg.Configurations;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Installer
{
    public class FFmpegProcessConfigurator : IFFmpegProcessConfigurator
    {
        private readonly FFmpegProcessConfiguratorContext _context;

        public FFmpegProcessConfigurator(FFmpegProcessConfiguratorContext context)
        {
            _context = context;
        }

        public IFFmpegProcessConfigurator ConfigureDefault(Action<FFmpegProcessConfiguration> configure)
        {
            configure.Invoke(_context.Configuration);
            return this;
        }

        public IFFmpegProcessConfigurator UseConfigurationResolver(IFFmpegProcessConfigurationResolver resolver)
        {
            _context.ConfigurationResolver = resolver;
            return this;
        }
    }

    public class FFmpegProcessConfiguratorContext
    {
        public FFmpegProcessConfiguration Configuration { get; }
        public IFFmpegProcessConfigurationResolver? ConfigurationResolver { get; set; }

        public FFmpegProcessConfiguratorContext(FFmpegProcessConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}
