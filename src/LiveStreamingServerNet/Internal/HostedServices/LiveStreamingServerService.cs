using LiveStreamingServerNet.Internal.HostedServices.Contracts;
using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.Extensions.Hosting;

namespace LiveStreamingServerNet.Internal.HostedService
{
    internal class LiveStreamingServerService : BackgroundService, ILiveStreamingServerService
    {
        private readonly IServer _server;
        private IReadOnlyList<ServerEndPoint>? _serverEndPoints;

        public LiveStreamingServerService(IServer server)
        {
            _server = server;
        }

        public void ConfigureEndPoints(IReadOnlyList<ServerEndPoint> serverEndPoints)
        {
            if (_server.IsStarted)
                throw new InvalidOperationException("Server has been started");

            _serverEndPoints = serverEndPoints;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_serverEndPoints == null || !_serverEndPoints.Any())
                throw new InvalidOperationException("Server end points are not configured");

            await _server.RunAsync(_serverEndPoints, stoppingToken);
        }
    }
}
