using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Services.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Transmuxer.Internal.Services
{
    internal class ServerEventHandler : IServerEventHandler
    {
        private readonly ITransmuxerManager _transmuxerManager;

        public ServerEventHandler(ITransmuxerManager transmuxerManager)
        {
            _transmuxerManager = transmuxerManager;
        }

        public async Task OnServerStoppedAsync()
        {
            await _transmuxerManager.DisposeAsync();
        }

        public Task OnClientAcceptedAsync(TcpClient tcpClient)
        {
            return Task.CompletedTask;
        }

        public Task OnClientConnectedAsync(IClientHandle client)
        {
            return Task.CompletedTask;
        }

        public Task OnClientDisconnectedAsync(IClientHandle client)
        {
            return Task.CompletedTask;
        }

        public Task OnListenerCreatedAsync(TcpListener tcpListener)
        {
            return Task.CompletedTask;
        }

        public Task OnServerStartedAsync()
        {
            return Task.CompletedTask;
        }
    }
}
