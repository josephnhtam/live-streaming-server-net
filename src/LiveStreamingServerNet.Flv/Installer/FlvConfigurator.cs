using LiveStreamingServerNet.Flv.Configurations;
using LiveStreamingServerNet.Flv.Contracts;
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

        public IFlvConfigurator Configure(Action<FlvConfiguration>? configure)
        {
            if (configure != null)
                _services.Configure(configure);

            return this;
        }

        public IFlvConfigurator ConfigureMediaStreaming(Action<MediaStreamingConfiguration>? configure)
        {
            if (configure != null)
                _services.Configure(configure);

            return this;
        }

        public IFlvConfigurator AddStreamEventHandler<TStreamEventHandler>()
            where TStreamEventHandler : class, IFlvServerStreamEventHandler
        {
            _services.AddSingleton<IFlvServerStreamEventHandler, TStreamEventHandler>();
            return this;
        }

        public IFlvConfigurator AddStreamEventHandler(Func<IServiceProvider, IFlvServerStreamEventHandler> implementationFactory)
        {
            _services.AddSingleton(implementationFactory);
            return this;
        }

    }
}
