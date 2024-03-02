using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpServerEventHandlers
{
    internal class RtmpServerConnectionEventDispatcher : IRtmpServerConnectionEventDispatcher
    {
        private readonly IServiceProvider _services;
        private IRtmpServerConnectionEventHandler[]? _eventHandlers;

        public RtmpServerConnectionEventDispatcher(IServiceProvider services)
        {
            _services = services;
        }

        public IRtmpServerConnectionEventHandler[] GetEventHandlers()
        {
            _eventHandlers ??= _services.GetServices<IRtmpServerConnectionEventHandler>().ToArray();
            return _eventHandlers;
        }

        public async ValueTask RtmpClientConnectedAsync(IRtmpClientContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments)
        {
            foreach (var eventHandler in GetEventHandlers())
                await eventHandler.OnRtmpClientConnectedAsync(clientContext, commandObject, arguments);
        }

        public async ValueTask RtmpClientCreatedAsync(IRtmpClientContext clientContext)
        {
            foreach (var eventHandler in GetEventHandlers())
                await eventHandler.OnRtmpClientCreatedAsync(clientContext);
        }

        public async ValueTask RtmpClientDisposedAsync(IRtmpClientContext clientContext)
        {
            foreach (var eventHandler in GetEventHandlers())
                await eventHandler.OnRtmpClientDisposedAsync(clientContext);
        }

        public async ValueTask RtmpClientHandshakeCompleteAsync(IRtmpClientContext clientId)
        {
            foreach (var eventHandler in GetEventHandlers())
                await eventHandler.OnRtmpClientHandshakeCompleteAsync(clientId);
        }
    }

    internal class RtmpServerStreamEventDispatcher : IRtmpServerStreamEventDispatcher
    {
        private readonly IServiceProvider _services;
        private IRtmpServerStreamEventHandler[]? _eventHandlers;

        public RtmpServerStreamEventDispatcher(IServiceProvider services)
        {
            _services = services;
        }

        public IRtmpServerStreamEventHandler[] GetEventHandlers()
        {
            _eventHandlers ??= _services.GetServices<IRtmpServerStreamEventHandler>().ToArray();
            return _eventHandlers;
        }

        public async ValueTask RtmpStreamMetaDataReceivedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            foreach (var eventHandler in GetEventHandlers())
                await eventHandler.OnRtmpStreamMetaDataReceivedAsync(clientContext, streamPath, metaData);
        }

        public async ValueTask RtmpStreamPublishedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            foreach (var eventHandler in GetEventHandlers())
                await eventHandler.OnRtmpStreamPublishedAsync(clientContext, streamPath, streamArguments);
        }

        public async ValueTask RtmpStreamSubscribedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            foreach (var eventHandler in GetEventHandlers())
                await eventHandler.OnRtmpStreamSubscribedAsync(clientContext, streamPath, streamArguments);
        }

        public async ValueTask RtmpStreamUnpublishedAsync(IRtmpClientContext clientContext, string streamPath)
        {
            foreach (var eventHandler in GetEventHandlers())
                await eventHandler.OnRtmpStreamUnpublishedAsync(clientContext, streamPath);
        }

        public async ValueTask RtmpStreamUnsubscribedAsync(IRtmpClientContext clientContext, string streamPath)
        {
            foreach (var eventHandler in GetEventHandlers())
                await eventHandler.OnRtmpStreamUnsubscribedAsync(clientContext, streamPath);
        }
    }
}
