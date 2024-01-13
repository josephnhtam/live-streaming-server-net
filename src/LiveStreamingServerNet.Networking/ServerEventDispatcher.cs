using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Newtorking.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Networking
{
    internal class ServerEventDispatcher : IServerEventDispatcher
    {
        private readonly IEnumerable<IServerEventHandler> _serverEventHandlers;

        public ServerEventDispatcher(IEnumerable<IServerEventHandler> serverEventHandlers)
        {
            _serverEventHandlers = serverEventHandlers;
        }

        public async Task ListenerCreatedAsync(TcpListener tcpListener)
        {
            foreach (var serverEventHandler in _serverEventHandlers)
                await serverEventHandler.OnListenerCreatedAsync(tcpListener);
        }

        public async Task ClientAcceptedAsync(TcpClient tcpClient)
        {
            foreach (var serverEventHandler in _serverEventHandlers)
                await serverEventHandler.OnClientAcceptedAsync(tcpClient);
        }

        public async Task ClientConnectedAsync(IClientHandle client)
        {
            foreach (var serverEventHandler in _serverEventHandlers)
                await serverEventHandler.OnClientConnectedAsync(client);
        }

        public async Task ClientDisconnectedAsync(IClientHandle client)
        {
            foreach (var serverEventHandler in _serverEventHandlers)
                await serverEventHandler.OnClientDisconnectedAsync(client);
        }

        public async Task ServerStartedAsync()
        {
            foreach (var serverEventHandler in _serverEventHandlers)
                await serverEventHandler.OnServerStartedAsync();
        }

        public async Task ServerStoppedAsync()
        {
            foreach (var serverEventHandler in _serverEventHandlers)
                await serverEventHandler.OnServerStoppedAsync();
        }
    }
}
