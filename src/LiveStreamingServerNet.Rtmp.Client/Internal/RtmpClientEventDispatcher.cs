using LiveStreamingClientNet.Rtmp.Client.Contracts;
using LiveStreamingClientNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Logging;
using LiveStreamingServerNet.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingClientNet.Rtmp.Client.Internal
{
    internal class RtmpClientConnectionEventDispatcher : IRtmpClientConnectionEventDispatcher
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private IRtmpClientConnectionEventHandler[]? _eventHandlers;

        public RtmpClientConnectionEventDispatcher(IServiceProvider services, ILogger<RtmpClientConnectionEventDispatcher> logger)
        {
            _services = services;
            _logger = logger;
        }

        public IRtmpClientConnectionEventHandler[] GetEventHandlers()
        {
            _eventHandlers ??= _services.GetServices<IRtmpClientConnectionEventHandler>().OrderBy(x => x.GetOrder()).ToArray();
            return _eventHandlers;
        }

        public async ValueTask RtmpConnectedAsync(
            IRtmpSessionContext sessionContext, IDictionary<string, object> commandObject, object? parameters)
        {
            try
            {
                using var eventContext = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.RtmpConnectedAsync(eventContext, commandObject, parameters);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpConnectedEventError(sessionContext.Session.Id, ex);
            }
        }

        public async ValueTask RtmpConnectionRejectedAsync(
            IRtmpSessionContext sessionContext, IDictionary<string, object> commandObject, object? parameters)
        {
            try
            {
                using var eventContext = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.RtmpConnectionRejectedAsync(eventContext, commandObject, parameters);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpConnectionRejectedEventError(sessionContext.Session.Id, ex);
            }
        }

        public async ValueTask RtmpHandshakeCompleteAsync(IRtmpSessionContext sessionContext)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpHandshakeCompleteAsync(context);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpHandshakeCompleteEventError(sessionContext.Session.Id, ex);
            }
        }
    }

    internal class RtmpClientStreamEventDispatcher : IRtmpClientStreamEventDispatcher
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private IRtmpClientStreamEventHandler[]? _eventHandlers;

        public RtmpClientStreamEventDispatcher(IServiceProvider services, ILogger<RtmpClientStreamEventDispatcher> logger)
        {
            _services = services;
            _logger = logger;
        }

        public IRtmpClientStreamEventHandler[] GetEventHandlers()
        {
            _eventHandlers ??= _services.GetServices<IRtmpClientStreamEventHandler>().OrderBy(x => x.GetOrder()).ToArray();
            return _eventHandlers;
        }

        public async ValueTask RtmpStreamCreated(IRtmpSessionContext sessionContext, uint streamId)
        {
            try
            {
                using var eventContext = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpStreamCreated(eventContext, streamId);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpStreamCreatedEventError(sessionContext.Session.Id, ex);
            }
        }
    }
}
