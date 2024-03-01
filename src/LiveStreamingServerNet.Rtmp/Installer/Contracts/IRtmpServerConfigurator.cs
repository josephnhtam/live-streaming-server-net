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

        IRtmpServerConfigurator AddAuthorizationHandler<TAuthorizationHandler>()
            where TAuthorizationHandler : class, IRtmpAuthorizationHandler;
        IRtmpServerConfigurator AddAuthorizationHandler(Func<IServiceProvider, IRtmpAuthorizationHandler> implmentationFactory);

        IRtmpServerConfigurator AddMediaMessageInterceptor<TMediaMessageInterceptor>()
            where TMediaMessageInterceptor : class, IRtmpMediaMessageInterceptor;
        IRtmpServerConfigurator AddMediaMessageInterceptor(Func<IServiceProvider, IRtmpMediaMessageInterceptor> implmentationFactory);

        IRtmpServerConfigurator AddConnectionEventHandler<TConnectionEventHandler>()
            where TConnectionEventHandler : class, IRtmpServerConnectionEventHandler;
        IRtmpServerConfigurator AddConnectionEventHandler(Func<IServiceProvider, IRtmpServerConnectionEventHandler> implmentationFactory);

        IRtmpServerConfigurator AddStreamEventHandler<TStreamEventHandler>()
            where TStreamEventHandler : class, IRtmpServerStreamEventHandler;
        IRtmpServerConfigurator AddStreamEventHandler(Func<IServiceProvider, IRtmpServerStreamEventHandler> implmentationFactory);

        IRtmpServerConfigurator AddBandwidthLimiter(long bytesPerSecond);
        IRtmpServerConfigurator AddBandwidthLimiter(long bytesPerSecond, long bytesLimit);
        IRtmpServerConfigurator AddBandwidthLimiter(Func<IServiceProvider, IBandwidthLimiter> factory);
    }
}
