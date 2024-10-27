using LiveStreamingServerNet.Networking.Client.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Installer;
using LiveStreamingServerNet.Rtmp.Client.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client
{
    public sealed partial class RtmpClientBuilder : IRtmpClientBuilder
    {
        private readonly IServiceCollection _builder;

        private Action<IRtmpClientConfigurator>? _configureRtmpClient;
        private Action<IClientConfigurator>? _configureClient;

        public IServiceCollection Services => _builder;

        private RtmpClientBuilder()
        {
            _builder = new ServiceCollection();
        }

        public IRtmpClientBuilder ConfigureLogging(Action<ILoggingBuilder> configure)
        {
            Services.AddLogging(configure);
            return this;
        }

        public IRtmpClientBuilder ConfigureRtmpClient(Action<IRtmpClientConfigurator> configure)
        {
            _configureRtmpClient = configure;
            return this;
        }

        public IRtmpClientBuilder ConfigureClient(Action<IClientConfigurator> configure)
        {
            _configureClient = configure;
            return this;
        }

        public IRtmpClient Build()
        {
            _builder.AddRtmpClient(_configureRtmpClient, _configureClient);

            var serviceProvider = Services.BuildServiceProvider();
            return new HostRtmpClient(serviceProvider);
        }
    }
}
