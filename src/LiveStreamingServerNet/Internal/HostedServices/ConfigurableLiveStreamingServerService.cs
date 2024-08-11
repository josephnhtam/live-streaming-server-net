using LiveStreamingServerNet.Internal.HostedServices.Contracts;
using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Server.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Internal.HostedServices
{
    internal class ConfigurableLiveStreamingServerService : BackgroundService, IConfigurableLiveStreamingServerService
    {
        private readonly IServer _server;
        private readonly ILogger _logger;
        private IReadOnlyList<ServerEndPoint>? _serverEndPoints;

        public ConfigurableLiveStreamingServerService(IServer server, ILogger<ConfigurableLiveStreamingServerService> logger)
        {
            _server = server;
            _logger = logger;
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
