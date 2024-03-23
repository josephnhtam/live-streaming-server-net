using LiveStreamingServerNet.Networking.Installer;
using LiveStreamingServerNet.Networking.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Auth;
using LiveStreamingServerNet.Rtmp.Auth.Contracts;
using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpServerEventHandlers;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

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
            services.AddServer<RtmpClientHandlerFactory>(options => configureServer?.Invoke(options))
                .AddTransient<IRtmpClientHandler, RtmpClientHandler>();

            services.AddMediatR(options =>
                options.RegisterServicesFromAssemblyContaining<RtmpClientHandler>());

            services.AddSingleton<Rtmp.Contracts.IRtmpServerContext, RtmpServerContext>();

            return services;
        }

        private static IServiceCollection AddRtmpServices(this IServiceCollection services)
        {
            services.AddSingleton<IRtmpChunkMessageSenderService, RtmpChunkMessageSenderService>()
                    .AddSingleton<IRtmpProtocolControlMessageSenderService, RtmpProtocolControlMessageSenderService>()
                    .AddSingleton<IRtmpUserControlMessageSenderService, RtmpUserControlMessageSenderService>()
                    .AddSingleton<IRtmpCommandMessageSenderService, RtmpCommandMessageSenderService>()
                    .AddSingleton<IRtmpMediaMessageManagerService, RtmpMediaMessageManagerService>()
                    .AddSingleton<IRtmpStreamManagerService, RtmpStreamManagerService>()
                    .AddSingleton<IRtmpStreamDeletionService, RtmpStreamDeletionService>()
                    .AddSingleton<IRtmpMediaMessageInterceptionService, RtmpMediaMessageInterceptionService>();

            return services;
        }

        private static IServiceCollection AddRtmpMessageHandlers(this IServiceCollection services)
        {
            var assembly = typeof(RtmpMessageDispatcher).Assembly;

            var handlerMap = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(IRtmpMessageHandler)))
                .Select(t => (HandlerType: t, MessageType: t.GetCustomAttributes<RtmpMessageTypeAttribute>()))
                .Where(x => x.MessageType.Any())
                .SelectMany(x => x.MessageType.Select(y => (x.HandlerType, MessageType: y)))
                .ToDictionary(x => x.MessageType!.MessageTypeId, x => x.HandlerType);

            foreach (var handlerType in handlerMap.Values)
            {
                services.AddSingleton(handlerType);
            }

            services.AddSingleton<IRtmpMessageHandlerMap>(new RtmpMessageHandlerMap(handlerMap))
                    .AddSingleton<IRtmpMessageDispatcher, RtmpMessageDispatcher>();

            return services;
        }

        private static IServiceCollection AddRtmpCommandHandlers(this IServiceCollection services)
        {
            var assembly = typeof(RtmpCommandDispatcher).Assembly;

            var handlerMap = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(RtmpCommandHandler)))
                .Select(t => (HandlerType: t, Command: t.GetCustomAttributes<RtmpCommandAttribute>()))
                .Where(x => x.Command.Any())
                .SelectMany(x => x.Command.Select(y => (x.HandlerType, Command: y)))
                .ToDictionary(x => x.Command!.Name, x => x.HandlerType);

            foreach (var handlerType in handlerMap.Values)
            {
                services.AddSingleton(handlerType);
            }

            services.AddSingleton<IRtmpCommandHanlderMap>(new RtmpCommandHandlerMap(handlerMap))
                    .AddSingleton<IRtmpCommandDispatcher, RtmpCommandDispatcher>();

            return services;
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
