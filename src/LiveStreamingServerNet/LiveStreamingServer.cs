using LiveStreamingServerNet.Internal.HostedServices.Contracts;
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
        private readonly ILiveStreamingServerService _serverService;

        public IServiceProvider Services => _server.Services;
        public bool IsStarted => _server.IsStarted;
        public IReadOnlyList<ServerEndPoint>? EndPoints => _server.EndPoints;
        public IReadOnlyList<IClientHandle> Clients => _server.Clients;

        public LiveStreamingServer(IHost host)
        {
            _host = host;
            _server = _host.Services.GetRequiredService<IServer>();
            _serverService = _host.Services.GetRequiredService<ILiveStreamingServerService>();
        }

        public IClientHandle? GetClient(uint clientId) => _server.GetClient(clientId);

        public Task RunAsync(ServerEndPoint serverEndPoint, CancellationToken cancellationToken = default)
            => RunAsync(new List<ServerEndPoint> { serverEndPoint }, cancellationToken);

        public async Task RunAsync(IReadOnlyList<ServerEndPoint> serverEndPoints, CancellationToken cancellationToken = default)
        {
            _serverService.ConfigureEndPoints(serverEndPoints);
            await _host.RunAsync(cancellationToken);
        }

        public void Dispose()
        {
            _host.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            _host.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
