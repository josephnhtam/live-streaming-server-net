using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpServerEventHandlers;
using LiveStreamingServerNet.Rtmp.Services;
using LiveStreamingServerNet.Rtmp.Services.Contracts;
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
                .AddRtmpCommandHandlers();
        }

        private static IServiceCollection AddRtmpCore(this IServiceCollection services)
        {
            services.AddServer<RtmpClientPeerHandlerFactory>()
                    .AddTransient<IRtmpClientPeerHandler, RtmpClientPeerHandler>();

            services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssemblyContaining<RtmpClientPeerHandler>();
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

            services.AddSingleton<IRtmpInternalServerEventHandler, RtmpClientPeerServerEventHandler>();

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
    }
}
