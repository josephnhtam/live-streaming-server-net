using LiveStreamingServerNet.Flv.Contracts;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Logging;
using LiveStreamingServerNet.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal class FlvServerStreamEventDispatcher : IFlvServerStreamEventDispatcher
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private IFlvServerStreamEventHandler[]? _eventHandlers;

        public FlvServerStreamEventDispatcher(IServiceProvider services, ILogger<FlvServerStreamEventDispatcher> logger)
        {
            _services = services;
            _logger = logger;
        }

        private IFlvServerStreamEventHandler[] GetEventHandlers()
        {
            _eventHandlers ??= _services.GetServices<IFlvServerStreamEventHandler>().OrderBy(x => x.GetOrder()).ToArray();
            return _eventHandlers;
        }

        public async ValueTask FlvStreamSubscribedAsync(IFlvClient client)
        {
            try
            {
                using var context = EventContext.Obtain();
                var clientHandle = new FlvClientHandle(client);

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnFlvStreamSubscribedAsync(context, clientHandle)
                        .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.DispatchingFlvStreamSubscribedEventError(client.ClientId, ex);
            }
        }

        public async ValueTask FlvStreamUnsubscribedAsync(IFlvClient client)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var eventHandler in GetEventHandlers())
                    await eventHandler.OnFlvStreamUnsubscribedAsync(context, client.ClientId, client.StreamPath).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.DispatchingFlvStreamUnsubscribedEventError(client.ClientId, ex);
            }
        }
    }
}
