using LiveStreamingServerNet.Flv.Configurations;
using LiveStreamingServerNet.Flv.Contracts;
using LiveStreamingServerNet.Flv.Installer.Contracts;
using LiveStreamingServerNet.Flv.Internal;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.HttpClients;
using LiveStreamingServerNet.Flv.Internal.HttpClients.Contracts;
using LiveStreamingServerNet.Flv.Internal.MediaPacketDiscarders;
using LiveStreamingServerNet.Flv.Internal.MediaPacketDiscarders.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Flv.Internal.WebSocketClients;
using LiveStreamingServerNet.Flv.Internal.WebSocketClients.Contracts;
using LiveStreamingServerNet.Flv.Middlewares;
using LiveStreamingServerNet.Networking.Server.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Installer.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Flv.Installer
{
    /// <summary>
    /// Static class providing extension methods for installing and configuring FLV streaming capabilities.
    /// </summary>
    public static class FlvInstaller
    {
        /// <summary>
        /// Adds FLV streaming support to the RTMP server configuration.
        /// </summary>
        /// <param name="configurator">The RTMP server configurator.</param>
        /// <returns>The RTMP server configurator for method chaining.</returns>
        public static IRtmpServerConfigurator AddFlv(this IRtmpServerConfigurator configurator)
            => AddFlv(configurator, null);

        /// <summary>
        /// Adds FLV streaming support to the RTMP server configuration.
        /// </summary>
        /// <param name="configurator">The RTMP server configurator.</param>
        /// <param name="configure">Optional action to configure FLV-specific settings.</param>
        /// <returns>The RTMP server configurator for method chaining.</returns>
        public static IRtmpServerConfigurator AddFlv(this IRtmpServerConfigurator configurator, Action<IFlvConfigurator>? configure)
        {
            var services = configurator.Services;

            configurator.AddStreamEventHandler<RtmpServerStreamEventListener>()
                .AddMediaMessageInterceptor<RtmpMediaMessageScraper>()
                .AddMediaCachingInterceptor<RtmpMediaCacheScraper>();

            services.AddSingleton<IFlvWriterFactory, FlvWriterFactory>()
                .AddSingleton<IFlvClientFactory, FlvClientFactory>()
                .AddSingleton<IFlvClientHandler, FlvClientHandler>()
                .AddSingleton<IMediaPacketDiscarderFactory, MediaPacketDiscarderFactory>();

            services.AddSingleton<IHttpFlvClientFactory, HttpFlvClientFactory>()
                .AddSingleton<IWebSocketFlvClientFactory, WebSocketFlvClientFactory>();

            services.AddSingleton<IFlvMediaTagSenderService, FlvMediaTagSenderService>()
                .AddSingleton<IFlvMediaTagCacherService, FlvMediaTagCacherService>()
                .AddSingleton<IFlvStreamManagerService, FlvStreamManagerService>()
                .AddSingleton<IFlvMediaTagBroadcasterService, FlvMediaTagBroadcasterService>();

            services.AddSingleton<IFlvServerStreamEventDispatcher, FlvServerStreamEventDispatcher>();

            services.AddSingleton<IFlvStreamInfoManager, FlvStreamInfoManager>();

            configure?.Invoke(new FlvConfigurator(services));

            return configurator;
        }

        /// <summary>
        /// Adds HTTP-FLV streaming middleware with a specified streaming server.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="liveStreamingServer">The live streaming server instance.</param>
        /// <returns>The application builder for method chaining.</returns>
        public static IApplicationBuilder UseHttpFlv(this IApplicationBuilder app, IServer liveStreamingServer)
            => UseHttpFlv(app, liveStreamingServer, null);

        /// <summary>
        /// Adds HTTP-FLV streaming middleware with a specified streaming server.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="liveStreamingServer">The live streaming server instance.</param>
        /// <param name="options">Optional HTTP-FLV configuration options.</param>
        /// <returns>The application builder for method chaining.</returns>
        public static IApplicationBuilder UseHttpFlv(this IApplicationBuilder app, IServer liveStreamingServer, HttpFlvOptions? options)
        {
            if (options == null)
                app.UseMiddleware<HttpFlvMiddleware>(liveStreamingServer);
            else
                app.UseMiddleware<HttpFlvMiddleware>(liveStreamingServer, Options.Create(options));

            return app;
        }

        /// <summary>
        /// Adds HTTP-FLV streaming middleware.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder for method chaining.</returns>
        public static IApplicationBuilder UseHttpFlv(this IApplicationBuilder app)
            => UseHttpFlv(app, options: null);

        /// <summary>
        /// Adds HTTP-FLV streaming middleware.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="options">Optional HTTP-FLV configuration options.</param>
        /// <returns>The application builder for method chaining.</returns>
        public static IApplicationBuilder UseHttpFlv(this IApplicationBuilder app, HttpFlvOptions? options)
        {
            if (options == null)
                app.UseMiddleware<HttpFlvMiddleware>();
            else
                app.UseMiddleware<HttpFlvMiddleware>(Options.Create(options));

            return app;
        }

        /// <summary>
        /// Adds WebSocket-FLV streaming middleware with a specified streaming server.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="liveStreamingServer">The live streaming server instance.</param>
        /// <returns>The application builder for method chaining.</returns>
        public static IApplicationBuilder UseWebSocketFlv(this IApplicationBuilder app, IServer liveStreamingServer)
            => UseWebSocketFlv(app, liveStreamingServer, null);

        /// <summary>
        /// Adds WebSocket-FLV streaming middleware with a specified streaming server.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="liveStreamingServer">The live streaming server instance.</param>
        /// <param name="options">Optional WebSocket-FLV configuration options.</param>
        /// <returns>The application builder for method chaining.</returns>
        public static IApplicationBuilder UseWebSocketFlv(this IApplicationBuilder app, IServer liveStreamingServer, WebSocketFlvOptions? options)
        {
            if (options == null)
                app.UseMiddleware<WebSocketFlvMiddleware>(liveStreamingServer);
            else
                app.UseMiddleware<WebSocketFlvMiddleware>(liveStreamingServer, Options.Create(options));

            return app;
        }

        /// <summary>
        /// Adds WebSocket-FLV streaming middleware.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder for method chaining.</returns>
        public static IApplicationBuilder UseWebSocketFlv(this IApplicationBuilder app)
            => UseWebSocketFlv(app, options: null);

        /// <summary>
        /// Adds WebSocket-FLV streaming middleware.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="options">Optional WebSocket-FLV configuration options.</param>
        /// <returns>The application builder for method chaining.</returns>
        public static IApplicationBuilder UseWebSocketFlv(this IApplicationBuilder app, WebSocketFlvOptions? options = null)
        {
            if (options == null)
                app.UseMiddleware<WebSocketFlvMiddleware>();
            else
                app.UseMiddleware<WebSocketFlvMiddleware>(Options.Create(options));

            return app;
        }
    }
}
