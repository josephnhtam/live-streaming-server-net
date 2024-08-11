using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Server.Contracts;
using LiveStreamingServerNet.Networking.Server.Internal.Contracts;
using LiveStreamingServerNet.Networking.Server.Internal.Logging;
using LiveStreamingServerNet.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Networking.Server.Internal
{
    internal class ServerEventDispatcher : IServerEventDispatcher
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private IServerEventHandler[]? _eventHandlers;

        public ServerEventDispatcher(IServiceProvider services, ILogger<ServerEventDispatcher> logger)
        {
            _services = services;
            _logger = logger;
        }

        public IServerEventHandler[] GetEventHandlers()
        {
            _eventHandlers ??= _services.GetServices<IServerEventHandler>().OrderBy(x => x.GetOrder()).ToArray();
            return _eventHandlers;
        }

        public async Task ListenerCreatedAsync(ITcpListener tcpListener)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var serverEventHandler in GetEventHandlers())
                    await serverEventHandler.OnListenerCreatedAsync(context, tcpListener);
            }
            catch (Exception ex)
            {
                _logger.DispatchingListenerCreatedEventError(ex);
            }
        }

        public async Task ClientAcceptedAsync(ITcpClient tcpClient)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var serverEventHandler in GetEventHandlers())
                    await serverEventHandler.OnClientAcceptedAsync(context, tcpClient);
            }
            catch (Exception ex)
            {
                _logger.DispatchingClientAcceptedEventError(ex);
            }
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

        public async Task ClientDisconnectedAsync(ISessionInfo client)
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var serverEventHandler in GetEventHandlers())
                    await serverEventHandler.OnClientDisconnectedAsync(context, client);
            }
            catch (Exception ex)
            {
                _logger.DispatchingClientDisconnectedEventError(ex);
            }
        }

        public async Task ServerStartedAsync()
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var serverEventHandler in GetEventHandlers())
                    await serverEventHandler.OnServerStartedAsync(context);
            }
            catch (Exception ex)
            {
                _logger.DispatchingServerStartedEventError(ex);
            }
        }

        public async Task ServerStoppedAsync()
        {
            try
            {
                using var context = EventContext.Obtain();

                foreach (var serverEventHandler in GetEventHandlers())
                    await serverEventHandler.OnServerStoppedAsync(context);
            }
            catch (Exception ex)
            {
                _logger.DispatchingServerStoppedEventError(ex);
            }
        }
    }
}
