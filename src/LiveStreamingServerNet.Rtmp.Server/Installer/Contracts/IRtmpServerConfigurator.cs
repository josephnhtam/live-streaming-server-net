using LiveStreamingServerNet.Rtmp.Server.Auth.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Configurations;
using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Rtmp.Server.RateLimiting.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Server.Installer.Contracts
{
    /// <summary>
    /// Configures an RTMP server instance and its dependencies.
    /// </summary>
    public interface IRtmpServerConfigurator
    {
        /// <summary>
        /// Gets the service collection for dependency injection configuration.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Configures core RTMP server settings.
        /// </summary>
        /// <param name="configure">Configuration action</param>
        IRtmpServerConfigurator Configure(Action<RtmpServerConfiguration>? configure);

        /// <summary>
        /// Configures media streaming settings.
        /// </summary>
        /// <param name="configure">Configuration action</param>
        IRtmpServerConfigurator ConfigureMediaStreaming(Action<MediaStreamingConfiguration>? configure);

        /// <summary>
        /// Adds an auth code provider implementation.
        /// </summary>
        /// <typeparam name="TAuthCodeProvider">Type of the auth code provider</typeparam>
        IRtmpServerConfigurator AddAuthCodeProvider<TAuthCodeProvider>()
            where TAuthCodeProvider : class, IAuthCodeProvider;

        /// <summary>
        /// Adds an auth code provider using a factory method.
        /// </summary>
        /// <param name="implementationFactory">Factory method to create the provider</param>
        IRtmpServerConfigurator AddAuthCodeProvider(Func<IServiceProvider, IAuthCodeProvider> implementationFactory);

        /// <summary>
        /// Adds an authorization handler implementation.
        /// </summary>
        /// <typeparam name="TAuthorizationHandler">Type of the authorization handler</typeparam>
        IRtmpServerConfigurator AddAuthorizationHandler<TAuthorizationHandler>()
            where TAuthorizationHandler : class, IAuthorizationHandler;

        /// <summary>
        /// Adds an authorization handler using a factory method.
        /// </summary>
        /// <param name="implementationFactory">Factory method to create the handler</param>
        IRtmpServerConfigurator AddAuthorizationHandler(Func<IServiceProvider, IAuthorizationHandler> implementationFactory);

        /// <summary>
        /// Adds a media message interceptor implementation.
        /// </summary>
        /// <typeparam name="TMediaMessageInterceptor">Type of the media message interceptor</typeparam>
        IRtmpServerConfigurator AddMediaMessageInterceptor<TMediaMessageInterceptor>()
            where TMediaMessageInterceptor : class, IRtmpMediaMessageInterceptor;

        /// <summary>
        /// Adds a media message interceptor using a factory method.
        /// </summary>
        /// <param name="implementationFactory">Factory method to create the interceptor</param>
        IRtmpServerConfigurator AddMediaMessageInterceptor(Func<IServiceProvider, IRtmpMediaMessageInterceptor> implementationFactory);

        /// <summary>
        /// Adds a media caching interceptor implementation.
        /// </summary>
        /// <typeparam name="TMediaCachingInterceptor">Type of the media caching interceptor</typeparam>
        IRtmpServerConfigurator AddMediaCachingInterceptor<TMediaCachingInterceptor>()
            where TMediaCachingInterceptor : class, IRtmpMediaCachingInterceptor;

        /// <summary>
        /// Adds a media caching interceptor using a factory method.
        /// </summary>
        /// <param name="implementationFactory">Factory method to create the interceptor</param>
        IRtmpServerConfigurator AddMediaCachingInterceptor(Func<IServiceProvider, IRtmpMediaCachingInterceptor> implementationFactory);

        /// <summary>
        /// Adds a connection event handler implementation.
        /// </summary>
        /// <typeparam name="TConnectionEventHandler">Type of the connection event handler</typeparam>
        IRtmpServerConfigurator AddConnectionEventHandler<TConnectionEventHandler>()
            where TConnectionEventHandler : class, IRtmpServerConnectionEventHandler;

        /// <summary>
        /// Adds a connection event handler using a factory method.
        /// </summary>
        /// <param name="implementationFactory">Factory method to create the handler</param>
        IRtmpServerConfigurator AddConnectionEventHandler(Func<IServiceProvider, IRtmpServerConnectionEventHandler> implementationFactory);

        /// <summary>
        /// Adds a stream event handler implementation.
        /// </summary>
        /// <typeparam name="TStreamEventHandler">Type of the stream event handler</typeparam>
        IRtmpServerConfigurator AddStreamEventHandler<TStreamEventHandler>()
            where TStreamEventHandler : class, IRtmpServerStreamEventHandler;

        /// <summary>
        /// Adds a stream event handler using a factory method.
        /// </summary>
        /// <param name="implementationFactory">Factory method to create the handler</param>
        IRtmpServerConfigurator AddStreamEventHandler(Func<IServiceProvider, IRtmpServerStreamEventHandler> implementationFactory);

        /// <summary>
        /// Adds a bandwidth limiter. Bytes are refilled at the specified rate.
        /// The maximum bytes limit defaults to ten second worth of bytes.
        /// When all available bytes are consumed, the publisher will be disconnected.
        /// </summary>
        /// <param name="bytesPerSecond">Byte refill rate per second</param>
        IRtmpServerConfigurator AddBandwidthLimiter(long bytesPerSecond);

        /// <summary>
        /// Adds a bandwidth limiter. Bytes are refilled at the specified rate.
        /// When all available bytes are consumed, the publisher will be disconnected.
        /// </summary>
        /// <param name="bytesPerSecond">Byte refill rate per second</param>
        /// <param name="bytesLimit">Maximum bytes allowed before disconnection</param>
        IRtmpServerConfigurator AddBandwidthLimiter(long bytesPerSecond, long bytesLimit);

        /// <summary>
        /// Adds bandwidth limiting using a custom factory.
        /// </summary>
        /// <param name="factory">Factory method to create the limiter</param>
        IRtmpServerConfigurator AddBandwidthLimiter(Func<IServiceProvider, IBandwidthLimiterFactory> factory);

        /// <summary>
        /// Configures video codec filtering.
        /// </summary>
        /// <param name="configure">Configuration action for the video codec filter</param>
        IRtmpServerConfigurator AddVideoCodecFilter(Action<IFilterBuilder<VideoCodec>> configure);

        /// <summary>
        /// Configures audio codec filtering.
        /// </summary>
        /// <param name="configure">Configuration action for the audio codec filter</param>
        IRtmpServerConfigurator AddAudioCodecFilter(Action<IFilterBuilder<AudioCodec>> configure);
    }
}
