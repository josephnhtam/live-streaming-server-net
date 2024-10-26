using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Server.Internal
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

        public async ValueTask RtmpClientDisposingAsync(IRtmpClientSessionContext clientContext)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpClientDisposingAsync(context, clientContext);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpClientDisposingEventError(clientContext.Client.Id, ex);
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

        public async ValueTask RtmpStreamMetaDataReceivedAsync(IRtmpPublishStreamContext publishStreamContext)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamMetaDataReceivedAsync(context, publishStreamContext);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamMetaDataReceivedEventError(publishStreamContext?.StreamContext?.ClientContext.Client.Id ?? 0, ex);
            }
        }

        public async ValueTask RtmpStreamPublishedAsync(IRtmpPublishStreamContext publishStreamContext)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamPublishedAsync(context, publishStreamContext);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamPublishedEventError(publishStreamContext?.StreamContext?.ClientContext.Client.Id ?? 0, ex);
            }
        }

        public async ValueTask RtmpStreamSubscribedAsync(IRtmpSubscribeStreamContext subscribeStreamContext)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamSubscribedAsync(context, subscribeStreamContext);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamSubscribedEventError(subscribeStreamContext.StreamContext.ClientContext.Client.Id, ex);
            }
        }

        public async ValueTask RtmpStreamUnpublishedAsync(IRtmpPublishStreamContext publishStreamContext, bool allowContinuation)
        {
            try
            {
                using var context = EventContext.Obtain();
                context.Items["AllowContinuation"] = allowContinuation;

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamUnpublishedAsync(context, publishStreamContext);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamUnpublishedEventError(publishStreamContext?.StreamContext?.ClientContext.Client.Id ?? 0, ex);
            }
        }

        public async ValueTask RtmpStreamUnsubscribedAsync(IRtmpSubscribeStreamContext subscribeStreamContext)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamUnsubscribedAsync(context, subscribeStreamContext);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamUnsubscribedEventError(subscribeStreamContext.StreamContext.ClientContext.Client.Id, ex);
            }
        }
    }
}
