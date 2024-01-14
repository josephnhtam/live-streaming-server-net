using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using IRtmpExternalServerConnectionEventHandler = LiveStreamingServerNet.Rtmp.Contracts.IRtmpServerConnectionEventHandler;
using IRtmpExternalServerStreamEventHandler = LiveStreamingServerNet.Rtmp.Contracts.IRtmpServerStreamEventHandler;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpServerEventHandlers
{
    internal class RtmpExternalServerConnectionEventDispatcher : IRtmpServerConnectionEventHandler
    {
        private readonly IEnumerable<IRtmpExternalServerConnectionEventHandler> _eventHandlers;

        public RtmpExternalServerConnectionEventDispatcher(IEnumerable<IRtmpExternalServerConnectionEventHandler> eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public async ValueTask OnRtmpClientConnectedAsync(IRtmpClientContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpClientConnectedAsync(clientContext.Client.ClientId, commandObject, arguments);
        }

        public async ValueTask OnRtmpClientCreatedAsync(IRtmpClientContext clientContext)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpClientCreatedAsync(clientContext.Client);
        }

        public async ValueTask OnRtmpClientDisposedAsync(IRtmpClientContext clientContext)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpClientDisposedAsync(clientContext.Client.ClientId);
        }

        public async ValueTask OnRtmpClientHandshakeCompleteAsync(IRtmpClientContext clientId)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpClientHandshakeCompleteAsync(clientId.Client.ClientId);
        }
    }

    internal class RtmpExternalServerStreamEventDispatcher : IRtmpServerStreamEventHandler
    {
        private readonly IEnumerable<IRtmpExternalServerStreamEventHandler> _eventHandlers;

        public RtmpExternalServerStreamEventDispatcher(IEnumerable<IRtmpExternalServerStreamEventHandler> eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public async ValueTask OnRtmpStreamMetaDataReceived(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpStreamMetaDataReceived(clientContext.Client.ClientId, streamPath, metaData);
        }

        public async ValueTask OnRtmpStreamPublishedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpStreamPublishedAsync(clientContext.Client.ClientId, streamPath, streamArguments);
        }

        public async ValueTask OnRtmpStreamSubscribedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpStreamSubscribedAsync(clientContext.Client.ClientId, streamPath, streamArguments);
        }

        public async ValueTask OnRtmpStreamUnpublishedAsync(IRtmpClientContext clientContext, string streamPath)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpStreamUnpublishedAsync(clientContext.Client.ClientId, streamPath);
        }

        public async ValueTask OnRtmpStreamUnsubscribedAsync(IRtmpClientContext clientContext, string streamPath)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpStreamUnsubscribedAsync(clientContext.Client.ClientId, streamPath);
        }
    }
}
