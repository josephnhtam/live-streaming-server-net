using LiveStreamingClientNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Client.Installer
{
    internal class RtmpClientConfigurator : IRtmpClientConfigurator
    {
        public IServiceCollection Services { get; }

        public RtmpClientConfigurator(IServiceCollection services)
        {
            Services = services;
        }

        public IRtmpClientConfigurator AddConnectionEventHandler<TConnectionEventHandler>()
            where TConnectionEventHandler : class, IRtmpClientConnectionEventHandler
        {
            Services.AddSingleton<IRtmpClientConnectionEventHandler, TConnectionEventHandler>();
            return this;
        }

        public IRtmpClientConfigurator AddConnectionEventHandler(Func<IServiceProvider, IRtmpClientConnectionEventHandler> implementationFactory)
        {
            Services.AddSingleton(implementationFactory);
            return this;
        }

        public IRtmpClientConfigurator AddStreamEventHandler<TStreamEventHandler>()
            where TStreamEventHandler : class, IRtmpClientStreamEventHandler
        {
            Services.AddSingleton<IRtmpClientStreamEventHandler, TStreamEventHandler>();
            return this;
        }

        public IRtmpClientConfigurator AddStreamEventHandler(Func<IServiceProvider, IRtmpClientStreamEventHandler> implementationFactory)
        {
            Services.AddSingleton(implementationFactory);
            return this;
        }
    }
}