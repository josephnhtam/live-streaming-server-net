using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpServerEventHandlers
{
    internal class RtmpServerConnectionEventDispatcher : IRtmpServerConnectionEventDispatcher
    {
        private readonly IEnumerable<IRtmpServerConnectionEventHandler> _eventHandlers;

        public RtmpServerConnectionEventDispatcher(IEnumerable<IRtmpServerConnectionEventHandler> eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public async Task RtmpClientConnectedAsync(IRtmpClientContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpClientConnectedAsync(clientContext, commandObject, arguments);
        }

        public async Task RtmpClientCreatedAsync(IRtmpClientContext clientContext)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpClientCreatedAsync(clientContext);
        }

        public async Task RtmpClientDisposedAsync(IRtmpClientContext clientContext)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpClientDisposedAsync(clientContext);
        }

        public async Task RtmpClientHandshakeCompleteAsync(IRtmpClientContext clientId)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpClientHandshakeCompleteAsync(clientId);
        }
    }

    internal class RtmpServerStreamEventDispatcher : IRtmpServerStreamEventDispatcher
    {
        private readonly IEnumerable<IRtmpServerStreamEventHandler> _eventHandlers;

        public RtmpServerStreamEventDispatcher(IEnumerable<IRtmpServerStreamEventHandler> eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public async Task RtmpStreamMetaDataReceived(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpStreamMetaDataReceived(clientContext, streamPath, metaData);
        }

        public async Task RtmpStreamPublishedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpStreamPublishedAsync(clientContext, streamPath, streamArguments);
        }

        public async Task RtmpStreamSubscribedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpStreamSubscribedAsync(clientContext, streamPath, streamArguments);
        }

        public async Task RtmpStreamUnpublishedAsync(IRtmpClientContext clientContext, string streamPath)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpStreamUnpublishedAsync(clientContext, streamPath);
        }

        public async Task RtmpStreamUnsubscribedAsync(IRtmpClientContext clientContext, string streamPath)
        {
            foreach (var eventHandler in _eventHandlers)
                await eventHandler.OnRtmpStreamUnsubscribedAsync(clientContext, streamPath);
        }
    }
}
