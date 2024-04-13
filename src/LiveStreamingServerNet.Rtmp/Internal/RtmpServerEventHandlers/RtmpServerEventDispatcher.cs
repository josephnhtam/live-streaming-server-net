using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Logging;
using LiveStreamingServerNet.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpServerEventHandlers
{
    internal class RtmpServerConnectionEventDispatcher : IRtmpServerConnectionEventDispatcher
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private IRtmpServerConnectionEventHandler[]? _eventHandlers;

        public RtmpServerConnectionEventDispatcher(IServiceProvider services, ILogger<RtmpServerConnectionEventDispatcher> logger)
        {
            _services = services;
            _logger = logger;
        }

        public IRtmpServerConnectionEventHandler[] GetEventHandlers()
        {
            _eventHandlers ??= _services.GetServices<IRtmpServerConnectionEventHandler>().OrderBy(x => x.GetOrder()).ToArray();
            return _eventHandlers;
        }

        public async ValueTask RtmpClientConnectedAsync(IRtmpClientContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientConnectedAsync(context, clientContext, commandObject, arguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientConnectedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask RtmpClientCreatedAsync(IRtmpClientContext clientContext)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientCreatedAsync(context, clientContext);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientCreatedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask RtmpClientDisposedAsync(IRtmpClientContext clientContext)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientDisposedAsync(context, clientContext);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientDisposedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask RtmpClientHandshakeCompleteAsync(IRtmpClientContext clientId)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientHandshakeCompleteAsync(context, clientId);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientHandshakeCompleteEventError(clientId.Client.ClientId, ex);
            }
        }
    }

    internal class RtmpServerStreamEventDispatcher : IRtmpServerStreamEventDispatcher
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private IRtmpServerStreamEventHandler[]? _eventHandlers;

        public RtmpServerStreamEventDispatcher(IServiceProvider services, ILogger<RtmpServerStreamEventDispatcher> logger)
        {
            _services = services;
            _logger = logger;
        }

        public IRtmpServerStreamEventHandler[] GetEventHandlers()
        {
            _eventHandlers ??= _services.GetServices<IRtmpServerStreamEventHandler>().OrderBy(x => x.GetOrder()).ToArray();
            return _eventHandlers;
        }

        public async ValueTask RtmpStreamMetaDataReceivedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamMetaDataReceivedAsync(context, clientContext, streamPath, metaData);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamMetaDataReceivedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask RtmpStreamPublishedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamPublishedAsync(context, clientContext, streamPath, streamArguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamPublishedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask RtmpStreamSubscribedAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamSubscribedAsync(context, clientContext, streamPath, streamArguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamSubscribedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask RtmpStreamUnpublishedAsync(IRtmpClientContext clientContext, string streamPath)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamUnpublishedAsync(context, clientContext, streamPath);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamUnpublishedEventError(clientContext.Client.ClientId, ex);
            }
        }

        public async ValueTask RtmpStreamUnsubscribedAsync(IRtmpClientContext clientContext, string streamPath)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamUnsubscribedAsync(context, clientContext, streamPath);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamUnsubscribedEventError(clientContext.Client.ClientId, ex);
            }
        }
    }
}
