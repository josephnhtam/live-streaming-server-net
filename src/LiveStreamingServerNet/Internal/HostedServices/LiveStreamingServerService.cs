using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Server.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Internal.HostedServices
{
    internal class LiveStreamingServerService : BackgroundService
    {
        private readonly IServer _server;
        private readonly ILogger _logger;
        private readonly IReadOnlyList<ServerEndPoint> _serverEndPoints;

        public LiveStreamingServerService(
            IServer server, ILogger<LiveStreamingServerService> logger, IReadOnlyList<ServerEndPoint> serverEndPoints)
        {
            _server = server;
            _logger = logger;
            _serverEndPoints = serverEndPoints;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _server.RunAsync(_serverEndPoints, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while running the live streaming server");
                throw;
            }
        }
    }
}
