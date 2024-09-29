using LiveStreamingServerNet.Networking.Client.Contracts;
using LiveStreamingServerNet.Networking.Client.Internal.Contracts;
using LiveStreamingServerNet.Networking.Client.Internal.Logging;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Networking.Client.Internal
{
    internal class ClientEventDispatcher : IClientEventDispatcher
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private IClientEventHandler[]? _eventHandlers;

        public ClientEventDispatcher(IServiceProvider services, ILogger<ClientEventDispatcher> logger)
        {
            _services = services;
            _logger = logger;
        }

        public IClientEventHandler[] GetEventHandlers()
        {
            _eventHandlers ??= _services.GetServices<IClientEventHandler>().OrderBy(x => x.GetOrder()).ToArray();
            return _eventHandlers;
        }

        public async Task ClientConnectedAsync(ISessionHandle client)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var serverEventHandler in GetEventHandlers())
                    await serverEventHandler.OnClientConnectedAsync(context, client);
            }
            catch (Exception ex)
            {
                _logger.DispatchingClientConnectedEventError(ex);
            }
        }

        public async Task ClientStoppedAsync()
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var serverEventHandler in GetEventHandlers())
                    await serverEventHandler.OnClientStoppedAsync(context);
            }
            catch (Exception ex)
            {
                _logger.DispatchingClientStoppedEventError(ex);
            }
        }
    }
}
