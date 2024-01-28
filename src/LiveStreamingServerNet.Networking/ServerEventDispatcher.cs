using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Networking
{
    internal class ServerEventDispatcher : IServerEventDispatcher
    {
        private readonly IServiceProvider _services;
        private IServerEventHandler[]? _eventHandlers;

        public ServerEventDispatcher(IServiceProvider services)
        {
            _services = services;
        }

        public IServerEventHandler[] GetEventHandlers()
        {
            _eventHandlers ??= _services.GetServices<IServerEventHandler>().ToArray();
            return _eventHandlers;
        }

        public async Task ListenerCreatedAsync(TcpListener tcpListener)
        {
            foreach (var serverEventHandler in GetEventHandlers())
                await serverEventHandler.OnListenerCreatedAsync(tcpListener);
        }

        public async Task ClientAcceptedAsync(TcpClient tcpClient)
        {
            foreach (var serverEventHandler in GetEventHandlers())
                await serverEventHandler.OnClientAcceptedAsync(tcpClient);
        }

        public async Task ClientConnectedAsync(IClientHandle client)
        {
            foreach (var serverEventHandler in GetEventHandlers())
                await serverEventHandler.OnClientConnectedAsync(client);
        }

        public async Task ClientDisconnectedAsync(IClientHandle client)
        {
            foreach (var serverEventHandler in GetEventHandlers())
                await serverEventHandler.OnClientDisconnectedAsync(client);
        }

        public async Task ServerStartedAsync()
        {
            foreach (var serverEventHandler in GetEventHandlers())
                await serverEventHandler.OnServerStartedAsync();
        }

        public async Task ServerStoppedAsync()
        {
            foreach (var serverEventHandler in GetEventHandlers())
                await serverEventHandler.OnServerStoppedAsync();
        }
    }
}
