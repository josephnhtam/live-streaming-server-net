using LiveStreamingServerNet.Builders.Contracts;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Newtorking;
using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.CommandDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.CommandDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.CommandDispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.MessageDispatcher;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.MessageDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.RtmpEventHandler.MessageDispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.ServerEventHandlers;
using LiveStreamingServerNet.Rtmp.Services;
using LiveStreamingServerNet.Rtmp.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace LiveStreamingServerNet.Builders
{
    public class LiveStreamingServerBuilder : ILiveStreamingServerBuilder
    {
        private readonly ServiceCollection _services;

        private LiveStreamingServerBuilder()
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
                     .AddSingleton<IRtmpCommandMessageSenderService, RtmpCommandMessageSenderService>()
                     .AddSingleton<IRtmpMediaMessageSenderService, RtmpMediaMessageSenderService>()
                     .AddSingleton<IRtmpStreamManagerService, RtmpStreamManagerService>();

            _services.AddSingleton<IRtmpServerEventHandler, RtmpServerEventHandler>();
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

        public static ILiveStreamingServerBuilder Create()
        {
            return new LiveStreamingServerBuilder();
        }

        public ILiveStreamingServerBuilder ConfigureLogging(Action<ILoggingBuilder> configureLogging)
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
