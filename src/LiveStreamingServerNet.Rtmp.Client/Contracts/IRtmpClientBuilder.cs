using LiveStreamingServerNet.Networking.Client.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Contracts
{
    public interface IRtmpClientBuilder
    {
        IServiceCollection Services { get; }
        IRtmpClientBuilder ConfigureLogging(Action<ILoggingBuilder> configure);
        IRtmpClientBuilder ConfigureRtmpClient(Action<IRtmpClientConfigurator> configure);
        IRtmpClientBuilder ConfigureClient(Action<IClientConfigurator> configure);
        IRtmpClient Build();
    }
}
