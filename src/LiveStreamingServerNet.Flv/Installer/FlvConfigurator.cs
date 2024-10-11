using LiveStreamingServerNet.Flv.Configurations;
using LiveStreamingServerNet.Flv.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Flv.Installer
{
    internal class FlvConfigurator : IFlvConfigurator
    {
        private readonly IServiceCollection _services;

        public FlvConfigurator(IServiceCollection services)
        {
            _services = services;
        }

        public IFlvConfigurator ConfigureMediaStreaming(Action<MediaStraemingConfiguration>? configure)
        {
            if (configure != null)
                _services.Configure(configure);

            return this;
        }
    }
}
