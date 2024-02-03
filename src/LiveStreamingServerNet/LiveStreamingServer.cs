using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet
{
    internal class LiveStreamingServer : ILiveStreamingServer
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IServer _server;

        public IServiceProvider Services => _server.Services;
        public bool IsStarted => _server.IsStarted;
        public IReadOnlyList<ServerEndPoint>? EndPoints => _server.EndPoints;
        public IReadOnlyList<IClientHandle> Clients => _server.Clients;
        public LiveStreamingServer(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _server = _serviceProvider.GetRequiredService<IServer>();
        }

        public IClientHandle? GetClient(uint clientId) => _server.GetClient(clientId);

        public Task RunAsync(ServerEndPoint serverEndPoint, CancellationToken cancellationToken = default)
            => _server.RunAsync(serverEndPoint, cancellationToken);

        public Task RunAsync(IList<ServerEndPoint> serverEndPoints, CancellationToken cancellationToken = default)
            => _server.RunAsync(serverEndPoints, cancellationToken);

        public ValueTask DisposeAsync()
            => _serviceProvider.DisposeAsync();
    }
}
