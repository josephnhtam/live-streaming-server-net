using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher
{
    internal static class Installer
    {
        public static IServiceCollection AddRtmpMessageHandlers(this IServiceCollection services, params Assembly[] assemblies)
        {
            return services.AddRtmpMessageHandlers(assemblies.SelectMany(x => x.GetTypes()).ToArray());
        }

        public static IServiceCollection AddRtmpMessageHandlers(this IServiceCollection services, params Type[] handlerTypes)
        {
            var handlerMap = handlerTypes
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
    }
}
