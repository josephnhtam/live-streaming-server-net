using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.Extensions.Hosting;

namespace LiveStreamingServerNet.Networking.Helpers
{
    public class BackgroundServerService : BackgroundService
    {
        private readonly IServer _server;
        private readonly ServerEndPoint[] _serverEndPoints;

        public BackgroundServerService(IServer server, params ServerEndPoint[] serverEndPoints)
        {
            _server = server;
            _serverEndPoints = serverEndPoints;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _server.RunAsync(_serverEndPoints, stoppingToken);
        }
    }
}
