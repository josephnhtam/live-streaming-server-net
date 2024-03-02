using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using Microsoft.Extensions.DependencyInjection;
using IRtmpExternalServerConnectionEventHandler = LiveStreamingServerNet.Rtmp.Contracts.IRtmpServerConnectionEventHandler;
using IRtmpExternalServerStreamEventHandler = LiveStreamingServerNet.Rtmp.Contracts.IRtmpServerStreamEventHandler;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpServerEventHandlers
{
    internal class RtmpExternalServerConnectionEventDispatcher : IRtmpServerConnectionEventHandler
    {
        private readonly IServiceProvider _services;
        private IRtmpExternalServerConnectionEventHandler[]? _eventHandlers;

        public RtmpExternalServerConnectionEventDispatcher(IServiceProvider services)
        {
            _services = services;
        }

        public IRtmpExternalServerConnectionEventHandler[] GetEventHandlers()
        {
            _eventHandlers ??= _services.GetServices<IRtmpExternalServerConnectionEventHandler>().ToArray();
            return _eventHandlers;
        }

        public async ValueTask OnRtmpClientConnectedAsync(IRtmpClientContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments)
        {
            foreach (var eventHandler in GetEventHandlers())
                await eventHandler.OnRtmpClientConnectedAsync(clientContext.Client.ClientId, commandObject, arguments);
        }

        public async ValueTask OnRtmpClientCreatedAsync(IRtmpClientContext clientContext)
        {
            foreach (var eventHandler in GetEventHandlers())
                await eventHandler.OnRtmpClientCreatedAsync(clientContext.Client);
        }

        public async ValueTask OnRtmpClientDisposedAsync(IRtmpClientContext clientContext)
        {
            foreach (var eventHandler in GetEventHandlers())
                await eventHandler.OnRtmpClientDisposedAsync(clientContext.Client.ClientId);
        }

        public async ValueTask OnRtmpClientHandshakeCompleteAsync(IRtmpClientContext clientId)
        {
            foreach (var eventHandler in GetEventHandlers())
                await eventHandler.OnRtmpClientHandshakeCompleteAsync(clientId.Client.ClientId);
        }
    }

    internal class RtmpExternalServerStreamEventDispatcher : IRtmpServerStreamEventHandler
    {
        private readonly IServiceProvider _services;
        private IRtmpExternalServerStreamEventHandler[]? _eventHandlers;

        public RtmpExternalServerStreamEventDispatcher(IServiceProvider services)
        {
            _services = services;
        }

        public IRtmpExternalServerStreamEventHandler[] GetEventHandlers()
        {
            _eventHandlers ??= _services.GetServices<IRtmpExternalServerStreamEventHandler>().ToArray();
            return _eventHandlers;
        }

        public async ValueTask OnRtmpStreamMetaDataReceivedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            foreach (var eventHandler in GetEventHandlers())
                await eventHandler.OnRtmpStreamMetaDataReceivedAsync(clientContext.Client.ClientId, streamPath, metaData);
        }

        public async ValueTask OnRtmpStreamPublishedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            foreach (var eventHandler in GetEventHandlers())
                await eventHandler.OnRtmpStreamPublishedAsync(clientContext.Client.ClientId, streamPath, streamArguments);
        }

        public async ValueTask OnRtmpStreamSubscribedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            foreach (var eventHandler in GetEventHandlers())
                await eventHandler.OnRtmpStreamSubscribedAsync(clientContext.Client.ClientId, streamPath, streamArguments);
        }

        public async ValueTask OnRtmpStreamUnpublishedAsync(IRtmpClientContext clientContext, string streamPath)
        {
            foreach (var eventHandler in GetEventHandlers())
                await eventHandler.OnRtmpStreamUnpublishedAsync(clientContext.Client.ClientId, streamPath);
        }

        public async ValueTask OnRtmpStreamUnsubscribedAsync(IRtmpClientContext clientContext, string streamPath)
        {
            foreach (var eventHandler in GetEventHandlers())
                await eventHandler.OnRtmpStreamUnsubscribedAsync(clientContext.Client.ClientId, streamPath);
        }
    }
}
