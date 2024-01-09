using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.Rtmp.Installer
{
    public class RtmpServerConfigurator : IRtmpServerConfigurator
    {
        public IServiceCollection Services { get; }

        public RtmpServerConfigurator(IServiceCollection services)
        {
            Services = services;
        }

        public IRtmpServerConfigurator ConfigureRtmpServer(Action<RtmpServerConfiguration>? configure)
        {
            if (configure != null)
                Services.Configure(configure);
            return this;
        }

        public IRtmpServerConfigurator ConfigureMediaMessage(Action<MediaMessageConfiguration>? configure)
        {
            if (configure != null)
                Services.Configure(configure);
            return this;
        }

        public IRtmpServerConfigurator AddAuthorizationHandler<TAuthorizationHandler>()
            where TAuthorizationHandler : class, IRtmpAuthorizationHandler
        {
            Services.TryAddSingleton<IRtmpAuthorizationHandler, TAuthorizationHandler>();
            return this;
        }

        public IRtmpServerConfigurator AddAuthorizationHandler(Func<IServiceProvider, IRtmpAuthorizationHandler> implmentationFactory)
        {
            Services.TryAddSingleton(implmentationFactory);
            return this;
        }

        public IRtmpServerConfigurator AddMediaMessageInterceptor<TMediaMessageInterceptor>()
            where TMediaMessageInterceptor : class, IRtmpMediaMessageInterceptor
        {
            Services.AddSingleton<IRtmpMediaMessageInterceptor, TMediaMessageInterceptor>();
            return this;
        }

        public IRtmpServerConfigurator AddMediaMessageInterceptor(Func<IServiceProvider, IRtmpMediaMessageInterceptor> implmentationFactory)
        {
            Services.AddSingleton(implmentationFactory);
            return this;
        }
    }
}
