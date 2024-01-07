using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using IRtmpExternalServerConnectionEventHandler = LiveStreamingServerNet.Rtmp.Contracts.IRtmpServerConnectionEventHandler;
using IRtmpExternalServerStreamEventHandler = LiveStreamingServerNet.Rtmp.Contracts.IRtmpServerStreamEventHandler;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpServerEvents
{
    internal class RtmpExternalServerConnectionEventDispatcher : IRtmpServerConnectionEventHandler
    {
        private readonly IEnumerable<IRtmpExternalServerConnectionEventHandler> _eventHandlers;

        public RtmpExternalServerConnectionEventDispatcher(IEnumerable<IRtmpExternalServerConnectionEventHandler> eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public async Task OnRtmpClientConnectedAsync(IRtmpClientContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpClientConnectedAsync(clientContext.Client.ClientId, commandObject, arguments);
        }

        public async Task OnRtmpClientCreatedAsync(IRtmpClientContext clientContext)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpClientCreatedAsync(clientContext.Client);
        }

        public async Task OnRtmpClientDisposedAsync(IRtmpClientContext clientContext)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpClientDisposedAsync(clientContext.Client.ClientId);
        }

        public async Task OnRtmpClientHandshakeCompleteAsync(IRtmpClientContext clientId)
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

        public async Task OnRtmpStreamMetaDataReceived(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpStreamMetaDataReceived(clientContext.Client.ClientId, streamPath, metaData);
        }

        public async Task OnRtmpStreamPublishedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpStreamPublishedAsync(clientContext.Client.ClientId, streamPath, streamArguments);
        }

        public async Task OnRtmpStreamSubscribedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpStreamSubscribedAsync(clientContext.Client.ClientId, streamPath, streamArguments);
        }

        public async Task OnRtmpStreamUnpublishedAsync(IRtmpClientContext clientContext, string streamPath)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpStreamUnpublishedAsync(clientContext.Client.ClientId, streamPath);
        }

        public async Task OnRtmpStreamUnsubscribedAsync(IRtmpClientContext clientContext, string streamPath)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpStreamUnsubscribedAsync(clientContext.Client.ClientId, streamPath);
        }
    }
}
