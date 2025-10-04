using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Installer
{
    public class AdaptiveHlsTranscoderConfigurator : IAdaptiveHlsTranscoderConfigurator
    {
        private readonly AdaptiveHlsTranscoderConfiguratorContext _context;

        public AdaptiveHlsTranscoderConfigurator(AdaptiveHlsTranscoderConfiguratorContext context)
        {
            _context = context;
        }

        public IAdaptiveHlsTranscoderConfigurator ConfigureDefault(Action<AdaptiveHlsTranscoderConfiguration> configure)
        {
            configure.Invoke(_context.Configuration);
            return this;
        }

        public IAdaptiveHlsTranscoderConfigurator UseConfigurationResolver(IAdaptiveHlsTranscoderConfigurationResolver resolver)
        {
            _context.ConfigurationResolver = resolver;
            return this;
        }
    }

    public class AdaptiveHlsTranscoderConfiguratorContext
    {
        public AdaptiveHlsTranscoderConfiguration Configuration { get; }
        public IAdaptiveHlsTranscoderConfigurationResolver? ConfigurationResolver { get; set; }

        public AdaptiveHlsTranscoderConfiguratorContext(AdaptiveHlsTranscoderConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}
