using LiveStreamingServerNet.Rtmp.Auth.Contracts;
using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Filtering;
using LiveStreamingServerNet.Rtmp.RateLimiting;
using LiveStreamingServerNet.Rtmp.RateLimiting.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.Rtmp.Installer
{
    internal class RtmpServerConfigurator : IRtmpServerConfigurator
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

        public IRtmpServerConfigurator AddAuthCodeProvider<TAuthCodeProvider>()
            where TAuthCodeProvider : class, IAuthCodeProvider
        {
            Services.TryAddSingleton<IAuthCodeProvider, TAuthCodeProvider>();
            return this;
        }

        public IRtmpServerConfigurator AddAuthCodeProvider(Func<IServiceProvider, IAuthCodeProvider> implementationFactory)
        {
            Services.TryAddSingleton(implementationFactory);
            return this;
        }

        public IRtmpServerConfigurator AddAuthorizationHandler<TAuthorizationHandler>()
            where TAuthorizationHandler : class, IAuthorizationHandler
        {
            Services.AddSingleton<IAuthorizationHandler, TAuthorizationHandler>();
            return this;
        }

        public IRtmpServerConfigurator AddAuthorizationHandler(Func<IServiceProvider, IAuthorizationHandler> implementationFactory)
        {
            Services.AddSingleton(implementationFactory);
            return this;
        }

        public IRtmpServerConfigurator AddMediaMessageInterceptor<TMediaMessageInterceptor>()
            where TMediaMessageInterceptor : class, IRtmpMediaMessageInterceptor
        {
            Services.AddSingleton<IRtmpMediaMessageInterceptor, TMediaMessageInterceptor>();
            return this;
        }

        public IRtmpServerConfigurator AddMediaMessageInterceptor(Func<IServiceProvider, IRtmpMediaMessageInterceptor> implementationFactory)
        {
            Services.AddSingleton(implementationFactory);
            return this;
        }

        public IRtmpServerConfigurator AddMediaCachingInterceptor<TMediaCachingInterceptor>()
            where TMediaCachingInterceptor : class, IRtmpMediaCachingInterceptor
        {
            Services.AddSingleton<IRtmpMediaCachingInterceptor, TMediaCachingInterceptor>();
            return this;
        }

        public IRtmpServerConfigurator AddMediaCachingInterceptor(Func<IServiceProvider, IRtmpMediaCachingInterceptor> implementationFactory)
        {
            Services.AddSingleton(implementationFactory);
            return this;
        }

        public IRtmpServerConfigurator AddConnectionEventHandler<TConnectionEventHandler>()
            where TConnectionEventHandler : class, IRtmpServerConnectionEventHandler
        {
            Services.AddSingleton<IRtmpServerConnectionEventHandler, TConnectionEventHandler>();
            return this;
        }

        public IRtmpServerConfigurator AddConnectionEventHandler(Func<IServiceProvider, IRtmpServerConnectionEventHandler> implementationFactory)
        {
            Services.AddSingleton(implementationFactory);
            return this;
        }

        public IRtmpServerConfigurator AddStreamEventHandler<TStreamEventHandler>()
            where TStreamEventHandler : class, IRtmpServerStreamEventHandler
        {
            Services.AddSingleton<IRtmpServerStreamEventHandler, TStreamEventHandler>();
            return this;
        }

        public IRtmpServerConfigurator AddStreamEventHandler(Func<IServiceProvider, IRtmpServerStreamEventHandler> implementationFactory)
        {
            Services.AddSingleton(implementationFactory);
            return this;
        }

        public IRtmpServerConfigurator AddBandwidthLimiter(long bytesPerSecond)
        {
            return AddBandwidthLimiter(bytesPerSecond, 10 * bytesPerSecond);
        }

        public IRtmpServerConfigurator AddBandwidthLimiter(long bytesPerSecond, long bytesLimit)
        {
            Services.TryAddSingleton<IBandwidthLimiterFactory>(_ => new BandwidthLimiterFactory(bytesPerSecond, bytesLimit));
            return this;
        }

        public IRtmpServerConfigurator AddBandwidthLimiter(Func<IServiceProvider, IBandwidthLimiterFactory> factory)
        {
            Services.TryAddSingleton(factory);
            return this;
        }

        public IRtmpServerConfigurator AddVideoCodecFilter(Action<IFilterBuilder<VideoCodec>> configure)
        {
            var builder = new FilterBuilder<VideoCodec>();
            configure(builder);
            Services.TryAddSingleton(builder.Build());
            return this;
        }

        public IRtmpServerConfigurator AddAudioCodecFilter(Action<IFilterBuilder<AudioCodec>> configure)
        {
            var builder = new FilterBuilder<AudioCodec>();
            configure(builder);
            Services.TryAddSingleton(builder.Build());
            return this;
        }
    }
}
