﻿using LiveStreamingServerNet.Flv.Configurations;
using LiveStreamingServerNet.Flv.Installer.Contracts;
using LiveStreamingServerNet.Flv.Internal;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.HttpClients;
using LiveStreamingServerNet.Flv.Internal.HttpClients.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Flv.Internal.WebSocketClients;
using LiveStreamingServerNet.Flv.Internal.WebSocketClients.Contracts;
using LiveStreamingServerNet.Flv.Middlewares;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Flv.Installer
{
    public static class FlvInstaller
    {
        public static IRtmpServerConfigurator AddFlv(this IRtmpServerConfigurator configurator, Action<IFlvConfigurator>? configure = null)
        {
            var services = configurator.Services;

            configurator.AddStreamEventHandler<RtmpServerStreamEventListener>()
                        .AddMediaMessageInterceptor<RtmpMediaMessageScraper>();

            services.AddTransient<IFlvClient, FlvClient>()
                    .AddTransient<IFlvWriter, FlvWriter>()
                    .AddSingleton<IFlvClientHandler, FlvClientHandler>();

            services.AddSingleton<IHttpFlvClientFactory, HttpFlvClientFactory>();

            services.AddSingleton<IWebSocketFlvClientFactory, WebSocketFlvClientFactory>();

            services.AddSingleton<IFlvStreamManagerService, FlvStreamManagerService>()
                    .AddSingleton<IFlvMediaTagManagerService, FlvMediaTagManagerService>();

            configure?.Invoke(new FlvConfigurator(services));

            return configurator;
        }

        public static void UseHttpFlv(this WebApplication webApplication, IServer liveStreamingServer, HttpFlvOptions? options = null)
        {
            webApplication.UseMiddleware<HttpFlvMiddleware>(liveStreamingServer, options ?? new HttpFlvOptions());
        }

        public static void UseWebSocketFlv(this WebApplication webApplication, IServer liveStreamingServer, WebSocketFlvOptions? options = null)
        {
            webApplication.UseMiddleware<WebSocketFlvMiddleware>(liveStreamingServer, options ?? new WebSocketFlvOptions());
        }
    }
}
