﻿using LiveStreamingServerNet.Networking.Server.Installer;
using LiveStreamingServerNet.Networking.Server.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Auth;
using LiveStreamingServerNet.Rtmp.Server.Auth.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal;
using LiveStreamingServerNet.Rtmp.Server.Internal.Authorization;
using LiveStreamingServerNet.Rtmp.Server.Internal.Authorization.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.MediaPackageDiscarding;
using LiveStreamingServerNet.Rtmp.Server.Internal.MediaPackageDiscarding.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpServerEventHandlers;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.Rtmp.Server.Installer
{
    public static class RtmpServerInstaller
    {
        public static IServiceCollection AddRtmpServer(
            this IServiceCollection services,
            Action<IRtmpServerConfigurator>? configureRtmpServer = null,
            Action<IServerConfigurator>? configureServer = null)
        {
            services.AddRtmpCore(configureServer)
                    .AddRtmpServices()
                    .AddRtmpMessageHandlers()
                    .AddRtmpCommandHandlers()
                    .AddRtmpServerEventDispatchers()
                    .AddRtmpServerEventHandlers();

            configureRtmpServer?.Invoke(new RtmpServerConfigurator(services));

            services.AddDefaults();

            return services;
        }

        private static IServiceCollection AddDefaults(this IServiceCollection services)
        {
            services.TryAddSingleton<IAuthCodeProvider, RandomHexAuthCodeProvider>();

            return services;
        }

        private static IServiceCollection AddRtmpCore(this IServiceCollection services, Action<IServerConfigurator>? configureServer)
        {
            services.AddServer<RtmpClientSessionHandlerFactory>(options => configureServer?.Invoke(options));

            services.AddMediator();

            services.AddSingleton<IRtmpServerContext, RtmpServerContext>()
                    .AddSingleton<IRtmpClientSessionContextFactory, RtmpClientSessionContextFactory>()
                    .AddSingleton<IMediaPackageDiscarderFactory, MediaPackageDiscarderFactory>()
                    .AddSingleton<IStreamAuthorization, StreamAuthorization>();

            return services;
        }

        private static IServiceCollection AddRtmpServices(this IServiceCollection services)
        {
            services.AddSingleton<IRtmpChunkMessageAggregatorService, RtmpChunkMessageAggregatorService>()
                    .AddSingleton<IRtmpChunkMessageWriterService, RtmpChunkMessageWriterService>();

            services.AddSingleton<IRtmpStreamManagerService, RtmpStreamManagerService>()
                    .AddSingleton<IRtmpChunkMessageSenderService, RtmpChunkMessageSenderService>()
                    .AddSingleton<IRtmpProtocolControlService, RtmpProtocolControlService>()
                    .AddSingleton<IRtmpUserControlMessageSenderService, RtmpUserControlMessageSenderService>()
                    .AddSingleton<IRtmpCommandMessageSenderService, RtmpCommandMessageSenderService>()
                    .AddSingleton<IRtmpMediaMessageCacherService, RtmpMediaMessageCacherService>()
                    .AddSingleton<IRtmpMediaMessageBroadcasterService, RtmpMediaMessageBroadcasterService>()
                    .AddSingleton<IRtmpStreamDeletionService, RtmpStreamDeletionService>()
                    .AddSingleton<IRtmpMediaMessageInterceptionService, RtmpMediaMessageInterceptionService>()
                    .AddSingleton<IRtmpMediaCachingInterceptionService, RtmpMediaCachingInterceptionService>();

            services.AddSingleton<IRtmpStreamInfoManager, RtmpStreamInfoManager>();

            return services;
        }

        private static IServiceCollection AddRtmpMessageHandlers(this IServiceCollection services)
        {
            return services.AddRtmpMessageHandlers<IRtmpClientSessionContext>(typeof(RtmpServerInstaller).Assembly);
        }

        private static IServiceCollection AddRtmpCommandHandlers(this IServiceCollection services)
        {
            return services.AddRtmpCommandHandlers<IRtmpClientSessionContext>(typeof(RtmpServerInstaller).Assembly);
        }

        private static IServiceCollection AddRtmpServerEventDispatchers(this IServiceCollection services)
        {
            services.AddSingleton<IRtmpServerConnectionEventDispatcher, RtmpServerConnectionEventDispatcher>()
                    .AddSingleton<IRtmpServerStreamEventDispatcher, RtmpServerStreamEventDispatcher>();

            return services;
        }

        private static IServiceCollection AddRtmpServerEventHandlers(this IServiceCollection services)
        {
            services.AddSingleton<Internal.Contracts.IRtmpServerConnectionEventHandler, RtmpServerConnectionEventHandler>()
                    .AddSingleton<Internal.Contracts.IRtmpServerConnectionEventHandler, RtmpExternalServerConnectionEventDispatcher>()
                    .AddSingleton<Internal.Contracts.IRtmpServerStreamEventHandler, RtmpExternalServerStreamEventDispatcher>();

            return services;
        }
    }
}
