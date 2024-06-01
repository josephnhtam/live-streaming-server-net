using LiveStreamingServerNet.Networking.Installer;
using LiveStreamingServerNet.Networking.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Auth;
using LiveStreamingServerNet.Rtmp.Auth.Contracts;
using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Authorization;
using LiveStreamingServerNet.Rtmp.Internal.Authorization.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.MediaPackageDiscarding;
using LiveStreamingServerNet.Rtmp.Internal.MediaPackageDiscarding.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpServerEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.Rtmp.Installer
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
            services.AddServer<RtmpClientHandlerFactory>(options => configureServer?.Invoke(options));

            services.AddMediator();

            services.AddSingleton<Rtmp.Contracts.IRtmpServerContext, RtmpServerContext>()
                    .AddSingleton<IRtmpClientContextFactory, RtmpClientContextFactory>()
                    .AddSingleton<IMediaPackageDiscarderFactory, MediaPackageDiscarderFactory>()
                    .AddSingleton<IStreamAuthorization, StreamAuthorization>();

            return services;
        }

        private static IServiceCollection AddRtmpServices(this IServiceCollection services)
        {
            services.AddSingleton<IRtmpChunkMessageWriterService, RtmpChunkMessageWriterService>()
                    .AddSingleton<IRtmpChunkMessageSenderService, RtmpChunkMessageSenderService>()
                    .AddSingleton<IRtmpProtocolControlMessageSenderService, RtmpProtocolControlMessageSenderService>()
                    .AddSingleton<IRtmpUserControlMessageSenderService, RtmpUserControlMessageSenderService>()
                    .AddSingleton<IRtmpCommandMessageSenderService, RtmpCommandMessageSenderService>()
                    .AddSingleton<IRtmpMediaMessageCacherService, RtmpMediaMessageCacherService>()
                    .AddSingleton<IRtmpMediaMessageBroadcasterService, RtmpMediaMessageBroadcasterService>()
                    .AddSingleton<IRtmpStreamManagerService, RtmpStreamManagerService>()
                    .AddSingleton<IRtmpStreamDeletionService, RtmpStreamDeletionService>()
                    .AddSingleton<IRtmpMediaMessageInterceptionService, RtmpMediaMessageInterceptionService>()
                    .AddSingleton<IRtmpMediaCachingInterceptionService, RtmpMediaCachingInterceptionService>();

            return services;
        }

        private static IServiceCollection AddRtmpMessageHandlers(this IServiceCollection services)
        {
            return services.AddRtmpMessageHandlers(typeof(RtmpMessageDispatcher).Assembly);
        }

        private static IServiceCollection AddRtmpCommandHandlers(this IServiceCollection services)
        {
            return services.AddRtmpCommandHandlers(typeof(RtmpCommandDispatcher).Assembly);
        }

        private static IServiceCollection AddRtmpServerEventDispatchers(this IServiceCollection services)
        {
            services.AddSingleton<IRtmpServerConnectionEventDispatcher, RtmpServerConnectionEventDispatcher>()
                    .AddSingleton<IRtmpServerStreamEventDispatcher, RtmpServerStreamEventDispatcher>();

            return services;
        }

        private static IServiceCollection AddRtmpServerEventHandlers(this IServiceCollection services)
        {
            services.AddSingleton<IRtmpServerConnectionEventHandler, RtmpServerConnectionEventHandler>()
                    .AddSingleton<IRtmpServerConnectionEventHandler, RtmpExternalServerConnectionEventDispatcher>()
                    .AddSingleton<IRtmpServerStreamEventHandler, RtmpExternalServerStreamEventDispatcher>();

            return services;
        }
    }
}
