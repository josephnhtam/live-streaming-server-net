using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpServerEvents
{
    internal class ServerEventHandler : IServerEventHandler
    {
        private readonly IRtmpMediaMessageManagerService _mediaMessageManager;

        public ServerEventHandler(IRtmpMediaMessageManagerService mediaMessageManager)
        {
            _mediaMessageManager = mediaMessageManager;
        }

        public async Task OnServerStoppedAsync()
        {
            await _mediaMessageManager.DisposeAsync();
        }

        public virtual Task OnClientAcceptedAsync(TcpClient tcpClient)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnClientConnectedAsync(IClientHandle client)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnClientDisconnectedAsync(IClientHandle client)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnListenerCreatedAsync(TcpListener tcpListener)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnServerStartedAsync()
        {
            return Task.CompletedTask;
        }
    }
}
