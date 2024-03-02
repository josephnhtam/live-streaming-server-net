using LiveStreamingServerNet.Rtmp.Auth.Contracts;
using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.RateLimiting;
using LiveStreamingServerNet.Rtmp.RateLimiting.Contracts;
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

        public IRtmpServerConfigurator Configure(Action<RtmpServerConfiguration>? configure)
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
            where TAuthorizationHandler : class, IAuthorizationHandler
        {
            Services.TryAddSingleton<IAuthorizationHandler, TAuthorizationHandler>();
            return this;
        }

        public IRtmpServerConfigurator AddAuthorizationHandler(Func<IServiceProvider, IAuthorizationHandler> implmentationFactory)
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

        public IRtmpServerConfigurator AddConnectionEventHandler<TConnectionEventHandler>()
            where TConnectionEventHandler : class, IRtmpServerConnectionEventHandler
        {
            Services.AddSingleton<IRtmpServerConnectionEventHandler, TConnectionEventHandler>();
            return this;
        }

        public IRtmpServerConfigurator AddConnectionEventHandler(Func<IServiceProvider, IRtmpServerConnectionEventHandler> implmentationFactory)
        {
            Services.AddSingleton(implmentationFactory);
            return this;
        }

        public IRtmpServerConfigurator AddStreamEventHandler<TStreamEventHandler>()
            where TStreamEventHandler : class, IRtmpServerStreamEventHandler
        {
            Services.AddSingleton<IRtmpServerStreamEventHandler, TStreamEventHandler>();
            return this;
        }

        public IRtmpServerConfigurator AddStreamEventHandler(Func<IServiceProvider, IRtmpServerStreamEventHandler> implmentationFactory)
        {
            Services.AddSingleton(implmentationFactory);
            return this;
        }

        public IRtmpServerConfigurator AddBandwidthLimiter(long bytesPerSecond)
        {
            return AddBandwidthLimiter(bytesPerSecond, 10 * bytesPerSecond);
        }

        public IRtmpServerConfigurator AddBandwidthLimiter(long bytesPerSecond, long bytesLimit)
        {
            Services.TryAddTransient<IBandwidthLimiter>(_ => new BandwidthLimiter(bytesPerSecond, bytesLimit));
            return this;
        }

        public IRtmpServerConfigurator AddBandwidthLimiter(Func<IServiceProvider, IBandwidthLimiter> factory)
        {
            Services.TryAddTransient(factory);
            return this;
        }
    }
}
