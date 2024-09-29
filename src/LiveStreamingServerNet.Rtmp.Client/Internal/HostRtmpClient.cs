using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Rtmp.Client.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal class HostRtmpClient : IRtmpClient
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IRtmpClient _innerClient;

        public HostRtmpClient(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _innerClient = serviceProvider.GetRequiredService<IRtmpClient>();
        }

        public bool IsHandshakeCompleted => _innerClient.IsHandshakeCompleted;
        public bool IsConnected => _innerClient.IsConnected;
        public bool IsStarted => _innerClient.IsStarted;
        public bool IsStopped => _innerClient.IsStopped;

        public Task<ConnectResponse> ConnectAsync(ServerEndPoint endPoint, string appName, IDictionary<string, object> information)
            => _innerClient.ConnectAsync(endPoint, appName, information);

        public Task<IRtmpStream> CreateStreamAsync()
            => _innerClient.CreateStreamAsync();

        public void Stop()
            => _innerClient.Stop();

        public Task UntilStoppedAsync()
            => _innerClient.UntilStoppedAsync();

        public async ValueTask DisposeAsync()
        {
            await _serviceProvider.DisposeAsync();
        }
    }
}
