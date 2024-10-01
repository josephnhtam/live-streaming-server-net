using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Rtmp.Client.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal class HostRtmpClient : IRtmpClient
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IRtmpClient _innerClient;

        public event EventHandler<BandwidthLimitEventArgs>? OnBandwidthLimitUpdated;

        public HostRtmpClient(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _innerClient = serviceProvider.GetRequiredService<IRtmpClient>();

            _innerClient.OnBandwidthLimitUpdated += (sender, e) => OnBandwidthLimitUpdated?.Invoke(sender, e);
        }

        public IServiceProvider Services => _innerClient.Services;
        public RtmpClientStatus Status => _innerClient.Status;

        public RtmpBandwidthLimit? BandwidthLimit => _innerClient.BandwidthLimit;

        public Task<ConnectResponse> ConnectAsync(ServerEndPoint endPoint, string appName)
            => _innerClient.ConnectAsync(endPoint, appName);

        public Task<ConnectResponse> ConnectAsync(ServerEndPoint endPoint, string appName, IDictionary<string, object> information)
            => _innerClient.ConnectAsync(endPoint, appName, information);

        public Task<IRtmpStream> CreateStreamAsync()
            => _innerClient.CreateStreamAsync();

        public void Stop()
            => _innerClient.Stop();

        public Task UntilStoppedAsync()
            => _innerClient.UntilStoppedAsync();

        public async ValueTask DisposeAsync()
            => await _serviceProvider.DisposeAsync();

        public void Command(RtmpCommand command)
            => _innerClient.Command(command);

        public Task<RtmpCommandResponse> CommandAsync(RtmpCommand command)
            => _innerClient.CommandAsync(command);
    }
}
