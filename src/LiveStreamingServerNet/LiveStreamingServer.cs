using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LiveStreamingServerNet
{
    internal class LiveStreamingServer : ILiveStreamingServer
    {
        private readonly IHost _host;
        private readonly IServer _server;

        public IServiceProvider Services => _server.Services;
        public bool IsStarted => _server.IsStarted;
        public IReadOnlyList<ServerEndPoint>? EndPoints => _server.EndPoints;
        public IReadOnlyList<IClientHandle> Clients => _server.Clients;

        public LiveStreamingServer(IHost host)
        {
            _host = host;
            _server = _host.Services.GetRequiredService<IServer>();
        }

        public IClientHandle? GetClient(uint clientId) => _server.GetClient(clientId);

        public Task RunAsync(ServerEndPoint serverEndPoint, CancellationToken cancellationToken = default)
            => _server.RunAsync(serverEndPoint, cancellationToken);

        public Task RunAsync(IList<ServerEndPoint> serverEndPoints, CancellationToken cancellationToken = default)
            => _server.RunAsync(serverEndPoints, cancellationToken);

        public ValueTask DisposeAsync()
        {
            _host.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
