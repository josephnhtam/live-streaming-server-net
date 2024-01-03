using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Services.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Rtmp.ServerEventHandlers
{
    public class RtmpServerEventHandler : IServerEventHandler
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

        public Task OnClientPeerConnectedAsync(IClientPeer clientPeer)
        {
            return Task.CompletedTask;
        }

        public Task OnClientPeerDisconnectedAsync(IClientPeer clientPeer)
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
