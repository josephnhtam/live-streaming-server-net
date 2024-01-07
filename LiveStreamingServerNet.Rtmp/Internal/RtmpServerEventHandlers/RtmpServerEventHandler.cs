using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpServerEventHandlers
{
    internal class RtmpServerEventHandler : IServerEventHandler
    {
        private readonly IRtmpMediaMessageManagerService _mediaMessageManager;

        public RtmpServerEventHandler(IRtmpMediaMessageManagerService mediaMessageManager)
        {
            _mediaMessageManager = mediaMessageManager;
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

        public async Task OnServerStoppedAsync()
        {
            await _mediaMessageManager.DisposeAsync();
        }
    }
}
