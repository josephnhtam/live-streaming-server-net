using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Logging;
using LiveStreamingServerNet.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal class RtmpHandshakeEventDispatcher : IRtmpHandshakeEventDispatcher
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private IRtmpHandshakeEventHandler[]? _eventHandlers;

        public RtmpHandshakeEventDispatcher(IServiceProvider services, ILogger<RtmpHandshakeEventDispatcher> logger)
        {
            _services = services;
            _logger = logger;
        }

        public IRtmpHandshakeEventHandler[] GetEventHandlers()
        {
            _eventHandlers ??= _services.GetServices<IRtmpHandshakeEventHandler>().OrderBy(x => x.GetOrder()).ToArray();
            return _eventHandlers;
        }

        public async ValueTask RtmpHandshakeCompleteAsync(IRtmpSessionContext sessionContext)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnRtmpHandshakeCompleteAsync(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.DispatchingRtmpHandshakeCompleteEventError(sessionContext.Session.Id, ex);
            }
        }
    }
}
