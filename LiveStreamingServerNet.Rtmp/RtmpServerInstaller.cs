using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.CommandDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.CommandDispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.MessageDispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.MessageDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.MessageDispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpServerEvents;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LiveStreamingServerNet.Rtmp
{
    public static class RtmpServerInstaller
    {
        public static IServiceCollection AddRtmpServer(this IServiceCollection services)
        {
            return services
                .AddRtmpCore()
                .AddRtmpServices()
                .AddRtmpMessageHandlers()
                .AddRtmpCommandHandlers()
                .AddRtmpServerEventDispatchers()
                .AddRtmpServerEventHandlers();
        }

        private static IServiceCollection AddRtmpCore(this IServiceCollection services)
        {
            services.AddServer<RtmpClientHandlerFactory>()
                    .AddTransient<IRtmpClientHandler, RtmpClientHandler>();

            services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssemblyContaining<RtmpClientHandler>();
            });

            services.AddSingleton<IRtmpServerContext, RtmpServerContext>();

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
                    .AddSingleton<IRtmpStreamDeletionService, RtmpStreamDeletionService>();

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

            services.AddSingleton<IRtmpMessageHanlderMap>(new RtmpMessageHanlderMap(handlerMap))
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

            services.AddSingleton<IRtmpCommandHanlderMap>(new RtmpCommandHanlderMap(handlerMap))
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
            services.AddSingleton<IServerEventHandler, ServerEventHandler>();

            services.AddSingleton<IRtmpServerConnectionEventHandler, RtmpServerConnectionEventHandler>()
                    .AddSingleton<IRtmpServerConnectionEventHandler, RtmpExternalServerConnectionEventDispatcher>();

            return services;
        }
    }
}
