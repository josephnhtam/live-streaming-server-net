using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Contracts;
using LiveStreamingServer.Rtmp.Core;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.MessageDispatcher.Contracts;
using LiveStreamingServer.Rtmp.Core.Services;
using LiveStreamingServer.Rtmp.Core.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace LiveStreamingServer.Rtmp
{
    public class RtmpServerBuilder : IRtmpServerBuilder
    {
        private readonly ServiceCollection _services;

        private RtmpServerBuilder()
        {
            _services = new ServiceCollection();

            _services.AddLogging();

            RegisterServer();
            RegisterRtmpCore();
            RegisterRtmpMessageHandlers();
            RegisterRtmpCommandHandlers();
            RegisterRtmpServices();
        }

        private void RegisterRtmpServices()
        {
            _services.AddSingleton<IRtmpChunkMessageSenderService, RtmpChunkMessageSenderService>()
                     .AddSingleton<IRtmpControlMessageSenderService, RtmpControlMessageSenderService>()
                     .AddSingleton<IRtmpCommandMessageSenderService, RtmpCommandMessageSenderService>();
        }

        private void RegisterServer()
        {
            _services.AddSingleton<IServer, Server>()
                     .AddTransient<IClientPeer, ClientPeer>()
                     .AddSingleton<INetBufferPool, NetBufferPool>();
        }

        private void RegisterRtmpCore()
        {
            _services.AddSingleton<IClientPeerHandlerFactory, RtmpClientPeerHandlerFactory>()
                     .AddTransient<IRtmpClientPeerHandler, RtmpClientPeerHandler>();

            _services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssemblyContaining<RtmpClientPeerHandler>();
            });

            _services.AddSingleton<IRtmpServerContext, RtmpServerContext>();
        }

        private void RegisterRtmpMessageHandlers()
        {
            var assembly = typeof(RtmpMessageDispatcher).Assembly;

            var handlerMap = assembly.GetExportedTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(IRtmpMessageHandler)))
                .Select(t => (HandlerType: t, MessageType: t.GetCustomAttributes<RtmpMessageTypeAttribute>()))
                .Where(x => x.MessageType.Any())
                .SelectMany(x => x.MessageType.Select(y => (x.HandlerType, MessageType: y)))
                .ToDictionary(x => x.MessageType!.MessageTypeId, x => x.HandlerType);

            foreach (var handlerType in handlerMap.Values)
            {
                _services.AddSingleton(handlerType);
            }

            _services.AddSingleton<IRtmpMessageHanlderMap>(new RtmpMessageHanlderMap(handlerMap))
                     .AddSingleton<IRtmpMessageDispatcher, RtmpMessageDispatcher>();
        }

        private void RegisterRtmpCommandHandlers()
        {
            var assembly = typeof(RtmpCommandDispatcher).Assembly;

            var handlerMap = assembly.GetExportedTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(RtmpCommandHandler)))
                .Select(t => (HandlerType: t, Command: t.GetCustomAttributes<RtmpCommandAttribute>()))
                .Where(x => x.Command.Any())
                .SelectMany(x => x.Command.Select(y => (x.HandlerType, Command: y)))
                .ToDictionary(x => x.Command!.Name, x => x.HandlerType);

            foreach (var handlerType in handlerMap.Values)
            {
                _services.AddSingleton(handlerType);
            }

            _services.AddSingleton<IRtmpCommandHanlderMap>(new RtmpCommandHanlderMap(handlerMap))
                     .AddSingleton<IRtmpCommandDispatcher, RtmpCommandDispatcher>();
        }

        public static IRtmpServerBuilder Create()
        {
            return new RtmpServerBuilder();
        }

        public IRtmpServerBuilder ConfigureLogging(Action<ILoggingBuilder> configureLogging)
        {
            _services.AddLogging(configureLogging);
            return this;
        }

        public IServer Build()
        {
            var provider = _services.BuildServiceProvider();
            return provider.GetRequiredService<IServer>();
        }
    }
}
