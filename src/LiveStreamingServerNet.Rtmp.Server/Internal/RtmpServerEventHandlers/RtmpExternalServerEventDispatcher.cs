using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IRtmpExternalServerConnectionEventHandler = LiveStreamingServerNet.Rtmp.Server.Contracts.IRtmpServerConnectionEventHandler;
using IRtmpExternalServerStreamEventHandler = LiveStreamingServerNet.Rtmp.Server.Contracts.IRtmpServerStreamEventHandler;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpServerEventHandlers
{
    internal class RtmpExternalServerConnectionEventDispatcher : IRtmpServerConnectionEventHandler
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private IRtmpExternalServerConnectionEventHandler[]? _eventHandlers;

        public RtmpExternalServerConnectionEventDispatcher(IServiceProvider services, ILogger<RtmpExternalServerConnectionEventDispatcher> logger)
        {
            _services = services;
            _logger = logger;
        }

        public IRtmpExternalServerConnectionEventHandler[] GetEventHandlers()
        {
            _eventHandlers ??= _services.GetServices<IRtmpExternalServerConnectionEventHandler>().OrderBy(x => x.GetOrder()).ToArray();
            return _eventHandlers;
        }

        public async ValueTask OnRtmpClientConnectedAsync(IEventContext context, IRtmpClientSessionContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientConnectedAsync(context, clientContext.Client.Id, commandObject, arguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientConnectedEventError(clientContext.Client.Id, ex);
            }
        }

        public async ValueTask OnRtmpClientCreatedAsync(IEventContext context, IRtmpClientSessionContext clientContext)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientCreatedAsync(context, clientContext.Client);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientCreatedEventError(clientContext.Client.Id, ex);
            }
        }

        public async ValueTask OnRtmpClientDisposingAsync(IEventContext context, IRtmpClientSessionContext clientContext)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientDisposingAsync(context, clientContext.Client.Id);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientDisposingEventError(clientContext.Client.Id, ex);
            }
        }

        public async ValueTask OnRtmpClientDisposedAsync(IEventContext context, IRtmpClientSessionContext clientContext)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientDisposedAsync(context, clientContext.Client.Id);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientDisposedEventError(clientContext.Client.Id, ex);
            }
        }

        public async ValueTask OnRtmpClientHandshakeCompleteAsync(IEventContext context, IRtmpClientSessionContext clientContext)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientHandshakeCompleteAsync(context, clientContext.Client.Id);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientHandshakeCompleteEventError(clientContext.Client.Id, ex);
            }
        }
    }

    internal class RtmpExternalServerStreamEventDispatcher : IRtmpServerStreamEventHandler
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private IRtmpExternalServerStreamEventHandler[]? _eventHandlers;

        public RtmpExternalServerStreamEventDispatcher(IServiceProvider services, ILogger<RtmpExternalServerStreamEventDispatcher> logger)
        {
            _services = services;
            _logger = logger;
        }

        public IRtmpExternalServerStreamEventHandler[] GetEventHandlers()
        {
            _eventHandlers ??= _services.GetServices<IRtmpExternalServerStreamEventHandler>().OrderBy(x => x.GetOrder()).ToArray();
            return _eventHandlers;
        }

        public async ValueTask OnRtmpStreamMetaDataReceivedAsync(IEventContext context, IRtmpPublishStreamContext publishStreamContext)
        {
            var clientId = publishStreamContext.StreamContext?.ClientContext.Client.Id ?? 0;
            var streamPath = publishStreamContext.StreamPath;
            var streamMetaData = publishStreamContext.StreamMetaData != null ?
                new Dictionary<string, object>(publishStreamContext.StreamMetaData) :
                new Dictionary<string, object>();

            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamMetaDataReceivedAsync(context, clientId, streamPath, streamMetaData);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamMetaDataReceivedEventError(clientId, ex);
            }
        }

        public async ValueTask OnRtmpStreamPublishedAsync(IEventContext context, IRtmpPublishStreamContext publishStreamContext)
        {
            var clientId = publishStreamContext.StreamContext?.ClientContext.Client.Id ?? 0;

            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamPublishedAsync(context, clientId, publishStreamContext.StreamPath, publishStreamContext.StreamArguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamPublishedEventError(clientId, ex);
            }
        }

        public async ValueTask OnRtmpStreamSubscribedAsync(IEventContext context, IRtmpSubscribeStreamContext subscribeStreamContext)
        {
            var clientId = subscribeStreamContext.StreamContext.ClientContext.Client.Id;

            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamSubscribedAsync(context, clientId, subscribeStreamContext.StreamPath, subscribeStreamContext.StreamArguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamSubscribedEventError(clientId, ex);
            }
        }

        public async ValueTask OnRtmpStreamUnpublishedAsync(IEventContext context, IRtmpPublishStreamContext publishStreamContext)
        {
            var clientId = publishStreamContext.StreamContext?.ClientContext.Client.Id ?? 0;

            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamUnpublishedAsync(context, clientId, publishStreamContext.StreamPath);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamUnpublishedEventError(clientId, ex);
            }
        }

        public async ValueTask OnRtmpStreamUnsubscribedAsync(IEventContext context, IRtmpSubscribeStreamContext subscribeStreamContext)
        {
            var clientId = subscribeStreamContext.StreamContext.ClientContext.Client.Id;

            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamUnsubscribedAsync(context, clientId, subscribeStreamContext.StreamPath);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamUnsubscribedEventError(clientId, ex);
            }
        }
    }
}