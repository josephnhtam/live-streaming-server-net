using LiveStreamingServerNet.Contracts;
using LiveStreamingServerNet.Internal.HostedService;
using LiveStreamingServerNet.Internal.HostedServices.Contracts;
using LiveStreamingServerNet.Networking.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Installer;
using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet
{
    public sealed class LiveStreamingServerBuilder : ILiveStreamingServerBuilder
    {
        private readonly HostApplicationBuilder _builder;

        private Action<IRtmpServerConfigurator>? _configureRtmpServer;
        private Action<IServerConfigurator>? _configureServer;

        public IServiceCollection Services => _builder.Services;

        private LiveStreamingServerBuilder()
        {
            _builder = Host.CreateEmptyApplicationBuilder(default);
        }

        public static ILiveStreamingServerBuilder Create()
        {
            return new LiveStreamingServerBuilder();
        }

        public ILiveStreamingServerBuilder ConfigureLogging(Action<ILoggingBuilder> configure)
        {
            Services.AddLogging(configure);
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

        public ILiveStreamingServer Build()
        {
            Services.AddRtmpServer(_configureRtmpServer, _configureServer);
            Services.AddSingleton<ILiveStreamingServerService, LiveStreamingServerService>();
            Services.AddHostedService(svc => svc.GetRequiredService<ILiveStreamingServerService>());

            Services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);

            return new LiveStreamingServer(_builder.Build());
        }
    }
}
