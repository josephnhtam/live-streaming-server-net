using LiveStreamingServerNet.Flv.Configurations;
using LiveStreamingServerNet.Flv.Installer.Contracts;
using LiveStreamingServerNet.Flv.Internal.HttpClients;
using LiveStreamingServerNet.Flv.Internal.HttpClients.Contracts;
using LiveStreamingServerNet.Flv.Internal.MediaPackageDiscarding;
using LiveStreamingServerNet.Flv.Internal.MediaPackageDiscarding.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Flv.Internal.WebSocketClients;
using LiveStreamingServerNet.Flv.Internal.WebSocketClients.Contracts;
using LiveStreamingServerNet.Flv.Middlewares;
using LiveStreamingServerNet.Networking.Server.Contracts;
using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Flv.Installer
{
    public static class FlvInstaller
    {
        public static IRtmpServerConfigurator AddFlv(this IRtmpServerConfigurator configurator, Action<IFlvConfigurator>? configure = null)
        {
            var services = configurator.Services;

            configurator.AddStreamEventHandler<RtmpServerStreamEventListener>()
                        .AddMediaMessageInterceptor<RtmpMediaMessageScraper>()
                        .AddMediaCachingInterceptor<RtmpMediaCacheScraper>();

            services.AddSingleton<IFlvWriterFactory, FlvWriterFactory>()
                    .AddSingleton<IFlvClientFactory, FlvClientFactory>()
                    .AddSingleton<IFlvClientHandler, FlvClientHandler>()
                    .AddSingleton<IMediaPackageDiscarderFactory, MediaPackageDiscarderFactory>();

            services.AddSingleton<IHttpFlvClientFactory, HttpFlvClientFactory>();

            services.AddSingleton<IWebSocketFlvClientFactory, WebSocketFlvClientFactory>();

            services.AddSingleton<IFlvMediaTagSenderService, FlvMediaTagSenderService>()
                    .AddSingleton<IFlvMediaTagCacherService, FlvMediaTagCacherService>()
                    .AddSingleton<IFlvStreamManagerService, FlvStreamManagerService>()
                    .AddSingleton<IFlvMediaTagBroadcasterService, FlvMediaTagBroadcasterService>();

            configure?.Invoke(new FlvConfigurator(services));

            return configurator;
        }

        public static IApplicationBuilder UseHttpFlv(this IApplicationBuilder app, IServer liveStreamingServer, HttpFlvOptions? options = null)
        {
            if (options == null)
                app.UseMiddleware<HttpFlvMiddleware>(liveStreamingServer);
            else
                app.UseMiddleware<HttpFlvMiddleware>(liveStreamingServer, Options.Create(options));

            return app;
        }

        public static IApplicationBuilder UseHttpFlv(this IApplicationBuilder app, HttpFlvOptions? options = null)
        {
            if (options == null)
                app.UseMiddleware<HttpFlvMiddleware>();
            else
                app.UseMiddleware<HttpFlvMiddleware>(Options.Create(options));

            return app;
        }

        public static IApplicationBuilder UseWebSocketFlv(this IApplicationBuilder app, IServer liveStreamingServer, WebSocketFlvOptions? options = null)
        {
            if (options == null)
                app.UseMiddleware<WebSocketFlvMiddleware>(liveStreamingServer);
            else
                app.UseMiddleware<WebSocketFlvMiddleware>(liveStreamingServer, Options.Create(options));

            return app;
        }

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
