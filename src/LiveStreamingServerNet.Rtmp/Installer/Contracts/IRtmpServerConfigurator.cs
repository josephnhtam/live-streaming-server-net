using LiveStreamingServerNet.Rtmp.Auth.Contracts;
using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RateLimiting.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Installer.Contracts
{
    public interface IRtmpServerConfigurator
    {
        IServiceCollection Services { get; }
        IRtmpServerConfigurator Configure(Action<RtmpServerConfiguration>? configure);
        IRtmpServerConfigurator ConfigureMediaMessage(Action<MediaMessageConfiguration>? configure);

        IRtmpServerConfigurator AddAuthCodeProvider<TAuthCodeProvider>()
            where TAuthCodeProvider : class, IAuthCodeProvider;
        IRtmpServerConfigurator AddAuthCodeProvider(Func<IServiceProvider, IAuthCodeProvider> implementationFactory);

        IRtmpServerConfigurator AddAuthorizationHandler<TAuthorizationHandler>()
            where TAuthorizationHandler : class, IAuthorizationHandler;
        IRtmpServerConfigurator AddAuthorizationHandler(Func<IServiceProvider, IAuthorizationHandler> implementationFactory);

        IRtmpServerConfigurator AddMediaMessageInterceptor<TMediaMessageInterceptor>()
            where TMediaMessageInterceptor : class, IRtmpMediaMessageInterceptor;
        IRtmpServerConfigurator AddMediaMessageInterceptor(Func<IServiceProvider, IRtmpMediaMessageInterceptor> implementationFactory);

        IRtmpServerConfigurator AddMediaCachingInterceptor<TMediaCachingInterceptor>()
            where TMediaCachingInterceptor : class, IRtmpMediaCachingInterceptor;
        IRtmpServerConfigurator AddMediaCachingInterceptor(Func<IServiceProvider, IRtmpMediaCachingInterceptor> implementationFactory);

        IRtmpServerConfigurator AddConnectionEventHandler<TConnectionEventHandler>()
            where TConnectionEventHandler : class, IRtmpServerConnectionEventHandler;
        IRtmpServerConfigurator AddConnectionEventHandler(Func<IServiceProvider, IRtmpServerConnectionEventHandler> implementationFactory);

        IRtmpServerConfigurator AddStreamEventHandler<TStreamEventHandler>()
            where TStreamEventHandler : class, IRtmpServerStreamEventHandler;
        IRtmpServerConfigurator AddStreamEventHandler(Func<IServiceProvider, IRtmpServerStreamEventHandler> implementationFactory);

        IRtmpServerConfigurator AddBandwidthLimiter(long bytesPerSecond);
        IRtmpServerConfigurator AddBandwidthLimiter(long bytesPerSecond, long bytesLimit);
        IRtmpServerConfigurator AddBandwidthLimiter(Func<IServiceProvider, IBandwidthLimiterFactory> factory);

        IRtmpServerConfigurator AddVideoCodecFilter(Action<IFilterBuilder<VideoCodec>> configure);
        IRtmpServerConfigurator AddAudioCodecFilter(Action<IFilterBuilder<AudioCodec>> configure);
    }
}
