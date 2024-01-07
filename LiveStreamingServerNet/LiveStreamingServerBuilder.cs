using LiveStreamingServerNet.Contracts;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Installer;
using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet
{
    public class LiveStreamingServerBuilder : ILiveStreamingServerBuilder
    {
        private readonly ServiceCollection _services;
        private Action<IRtmpServerConfigurator>? _configureRtmpServer;
        private Action<IServerConfigurator>? _configureServer;

        public IServiceCollection Services => _services;

        private LiveStreamingServerBuilder()
        {
            _services = new ServiceCollection();
        }

        public static ILiveStreamingServerBuilder Create()
        {
            return new LiveStreamingServerBuilder();
        }

        public ILiveStreamingServerBuilder ConfigureLogging(Action<ILoggingBuilder> configure)
        {
            _services.AddLogging(configure);
            return this;
        }

        public ILiveStreamingServerBuilder ConfigureRtmpServer(Action<IRtmpServerConfigurator> configure)
        {
            _configureRtmpServer = configure;
            return this;
        }

        public ILiveStreamingServerBuilder ConfigureServer(Action<IServerConfigurator> configure)
        {
            _configureServer = configure;
            return this;
        }

        public IServer Build()
        {
            _services.AddRtmpServer(_configureRtmpServer, _configureServer);

            var provider = _services.BuildServiceProvider();
            return provider.GetRequiredService<IServer>();
        }
    }
}
