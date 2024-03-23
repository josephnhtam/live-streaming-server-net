using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Logging;
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

        public async ValueTask OnRtmpClientConnectedAsync(IRtmpClientContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientConnectedAsync(clientContext.Client.ClientId, commandObject, arguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientConnectedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask OnRtmpClientCreatedAsync(IRtmpClientContext clientContext)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientCreatedAsync(clientContext.Client);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientCreatedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask OnRtmpClientDisposedAsync(IRtmpClientContext clientContext)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientDisposedAsync(clientContext.Client.ClientId);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientDisposedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask OnRtmpClientHandshakeCompleteAsync(IRtmpClientContext clientId)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientHandshakeCompleteAsync(clientId.Client.ClientId);
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

        public async ValueTask OnRtmpStreamMetaDataReceivedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamMetaDataReceivedAsync(clientContext.Client.ClientId, streamPath, metaData);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamMetaDataReceivedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask OnRtmpStreamPublishedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamPublishedAsync(clientContext.Client.ClientId, streamPath, streamArguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamPublishedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask OnRtmpStreamSubscribedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamSubscribedAsync(clientContext.Client.ClientId, streamPath, streamArguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamSubscribedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask OnRtmpStreamUnpublishedAsync(IRtmpClientContext clientContext, string streamPath)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamUnpublishedAsync(clientContext.Client.ClientId, streamPath);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamUnpublishedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask OnRtmpStreamUnsubscribedAsync(IRtmpClientContext clientContext, string streamPath)
        {
            try
            {
                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamUnsubscribedAsync(clientContext.Client.ClientId, streamPath);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamUnsubscribedEventError(clientContext.Client.ClientId, ex);
            }
        }
    }
}
