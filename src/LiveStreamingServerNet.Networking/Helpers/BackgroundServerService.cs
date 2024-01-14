using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace LiveStreamingServerNet.Networking.Helpers
{
    public class BackgroundServerService : BackgroundService
    {
        private readonly IServer _server;
        private readonly IPEndPoint _serverEndPoint;

        public BackgroundServerService(IServer server, IPEndPoint serverEndPoint)
        {
            _server = server;
            _serverEndPoint = serverEndPoint;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _server.RunAsync(_serverEndPoint, stoppingToken);
        }
    }
}
