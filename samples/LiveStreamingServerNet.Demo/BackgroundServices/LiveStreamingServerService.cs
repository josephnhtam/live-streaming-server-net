using LiveStreamingServerNet.Networking.Contracts;
using System.Net;

namespace LiveStreamingServerNet.Demo.BackgroundServices
{
    public class LiveStreamingServerService : BackgroundService
    {
        private IServer _liveStreamingServer;

        public LiveStreamingServerService([FromKeyedServices("live-streaming")] IServer liveStreamingServer)
        {
            _liveStreamingServer = liveStreamingServer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _liveStreamingServer.RunAsync(new IPEndPoint(IPAddress.Any, 1935), stoppingToken);
        }
    }
}
