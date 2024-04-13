using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Logging;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IRtmpExternalServerConnectionEventHandler = LiveStreamingServerNet.Rtmp.Contracts.IRtmpServerConnectionEventHandler;
using IRtmpExternalServerStreamEventHandler = LiveStreamingServerNet.Rtmp.Contracts.IRtmpServerStreamEventHandler;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpServerEventHandlers
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

        public async ValueTask OnRtmpClientConnectedAsync(IEventContext context, IRtmpClientContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientConnectedAsync(context, clientContext.Client.ClientId, commandObject, arguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientConnectedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask OnRtmpClientCreatedAsync(IEventContext context, IRtmpClientContext clientContext)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientCreatedAsync(context, clientContext.Client);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientCreatedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask OnRtmpClientDisposedAsync(IEventContext context, IRtmpClientContext clientContext)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientDisposedAsync(context, clientContext.Client.ClientId);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientDisposedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask OnRtmpClientHandshakeCompleteAsync(IEventContext context, IRtmpClientContext clientId)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientHandshakeCompleteAsync(context, clientId.Client.ClientId);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientHandshakeCompleteEventError(clientId.Client.ClientId, ex);
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

        public async ValueTask OnRtmpStreamMetaDataReceivedAsync(IEventContext context, IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamMetaDataReceivedAsync(context, clientContext.Client.ClientId, streamPath, metaData);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamMetaDataReceivedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask OnRtmpStreamPublishedAsync(IEventContext context, IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamPublishedAsync(context, clientContext.Client.ClientId, streamPath, streamArguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamPublishedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask OnRtmpStreamSubscribedAsync(IEventContext context, IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamSubscribedAsync(context, clientContext.Client.ClientId, streamPath, streamArguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamSubscribedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask OnRtmpStreamUnpublishedAsync(IEventContext context, IRtmpClientContext clientContext, string streamPath)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamUnpublishedAsync(context, clientContext.Client.ClientId, streamPath);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamUnpublishedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask OnRtmpStreamUnsubscribedAsync(IEventContext context, IRtmpClientContext clientContext, string streamPath)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamUnsubscribedAsync(context, clientContext.Client.ClientId, streamPath);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamUnsubscribedEventError(clientContext.Client.ClientId, ex);
            }
        }
    }
}
