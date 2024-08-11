using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Logging;
using LiveStreamingServerNet.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal
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

        public async ValueTask RtmpClientConnectedAsync(IRtmpClientSessionContext clientContext, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientConnectedAsync(context, clientContext, commandObject, arguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientConnectedEventError(clientContext.Client.Id, ex);
            }
        }

        public async ValueTask RtmpClientCreatedAsync(IRtmpClientSessionContext clientContext)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientCreatedAsync(context, clientContext);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientCreatedEventError(clientContext.Client.Id, ex);
            }
        }

        public async ValueTask RtmpClientDisposedAsync(IRtmpClientSessionContext clientContext)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientDisposedAsync(context, clientContext);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientDisposedEventError(clientContext.Client.Id, ex);
            }
        }

        public async ValueTask RtmpClientHandshakeCompleteAsync(IRtmpClientSessionContext clientContext)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientHandshakeCompleteAsync(context, clientContext);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientHandshakeCompleteEventError(clientContext.Client.Id, ex);
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

        public async ValueTask RtmpStreamMetaDataReceivedAsync(IRtmpClientSessionContext clientContext, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamMetaDataReceivedAsync(context, clientContext, streamPath, metaData);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamMetaDataReceivedEventError(clientContext.Client.Id, ex);
            }
        }

        public async ValueTask RtmpStreamPublishedAsync(IRtmpClientSessionContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamPublishedAsync(context, clientContext, streamPath, streamArguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamPublishedEventError(clientContext.Client.Id, ex);
            }
        }

        public async ValueTask RtmpStreamSubscribedAsync(IRtmpClientSessionContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamSubscribedAsync(context, clientContext, streamPath, streamArguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamSubscribedEventError(clientContext.Client.Id, ex);
            }
        }

        public async ValueTask RtmpStreamUnpublishedAsync(IRtmpClientSessionContext clientContext, string streamPath)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamUnpublishedAsync(context, clientContext, streamPath);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamUnpublishedEventError(clientContext.Client.Id, ex);
            }
        }

        public async ValueTask RtmpStreamUnsubscribedAsync(IRtmpClientSessionContext clientContext, string streamPath)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamUnsubscribedAsync(context, clientContext, streamPath);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamUnsubscribedEventError(clientContext.Client.Id, ex);
            }
        }
    }
}
