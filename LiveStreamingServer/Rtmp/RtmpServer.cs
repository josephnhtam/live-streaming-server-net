using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core;
using LiveStreamingServer.Rtmp.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace LiveStreamingServer.Rtmp
{
    public class RtmpServer : IServer
    {
        private readonly IServer _server;

        public RtmpServer()
        {
            var services = new ServiceCollection();

            services.AddLogging();

            services.AddSingleton<IServer, Server>()
                    .AddTransient<IClientPeer, ClientPeer>()
                    .AddSingleton<INetBufferPool, NetBufferPool>()
                    .AddSingleton<IClientPeerHandlerFactory, RtmpClientPeerHandlerFactory>()
                    .AddTransient<IRtmpClientPeerHandler, RtmpClientPeerHandler>();

            services.AddSingleton<IRtmpServerContext, RtmpServerContext>();

            var provider = services.BuildServiceProvider();
            _server = provider.GetRequiredService<IServer>();
        }

        public bool IsStarted => _server.IsStarted;
        public IList<IClientPeerHandle> ClientPeers => _server.ClientPeers;
        public IClientPeerHandle? GetClientPeer(uint clientPeerId) => _server.GetClientPeer(clientPeerId);
        public Task RunAsync(IPEndPoint localEndpoint, CancellationToken cancellationToken = default)
            => _server.RunAsync(localEndpoint, cancellationToken);
    }
}
