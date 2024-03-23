using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Networking
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

        public async Task ListenerCreatedAsync(TcpListener tcpListener)
        {
            try
            {
                foreach (var serverEventHandler in GetEventHandlers())
                    await serverEventHandler.OnListenerCreatedAsync(tcpListener);
            }
            catch (Exception ex)
            {
                _logger.DispatchingListenerCreatedEventError(ex);
            }
        }

        public async Task ClientAcceptedAsync(TcpClient tcpClient)
        {
            try
            {
                foreach (var serverEventHandler in GetEventHandlers())
                    await serverEventHandler.OnClientAcceptedAsync(tcpClient);
            }
            catch (Exception ex)
            {
                _logger.DispatchingClientAcceptedEventError(ex);
            }
        }

        public async Task ClientConnectedAsync(IClientHandle client)
        {
            try
            {
                foreach (var serverEventHandler in GetEventHandlers())
                    await serverEventHandler.OnClientConnectedAsync(client);
            }
            catch (Exception ex)
            {
                _logger.DispatchingClientConnectedEventError(ex);
            }
        }

        public async Task ClientDisconnectedAsync(IClientHandle client)
        {
            try
            {
                foreach (var serverEventHandler in GetEventHandlers())
                    await serverEventHandler.OnClientDisconnectedAsync(client);
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
                foreach (var serverEventHandler in GetEventHandlers())
                    await serverEventHandler.OnServerStartedAsync();
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
                foreach (var serverEventHandler in GetEventHandlers())
                    await serverEventHandler.OnServerStoppedAsync();
            }
            catch (Exception ex)
            {
                _logger.DispatchingServerStoppedEventError(ex);
            }
        }
    }
}
