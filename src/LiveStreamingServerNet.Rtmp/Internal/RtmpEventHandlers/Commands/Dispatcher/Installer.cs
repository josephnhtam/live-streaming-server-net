using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher
{
    internal static class Installer
    {
        public static IServiceCollection AddRtmpCommandHandlers(this IServiceCollection services, params Assembly[] assemblies)
        {
            return services.AddRtmpCommandHandlers(assemblies.SelectMany(x => x.GetTypes()).ToArray());
        }

        public static IServiceCollection AddRtmpCommandHandlers(this IServiceCollection services, params Type[] handlerTypes)
        {
            var handlerMap = handlerTypes
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
    }
}
