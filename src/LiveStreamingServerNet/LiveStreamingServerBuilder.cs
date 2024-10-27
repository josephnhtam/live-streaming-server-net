using LiveStreamingServerNet.Contracts;
using LiveStreamingServerNet.Internal.HostedServices;
using LiveStreamingServerNet.Internal.HostedServices.Contracts;
using LiveStreamingServerNet.Networking.Server.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Installer;
using LiveStreamingServerNet.Rtmp.Server.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet
{
    public sealed partial class LiveStreamingServerBuilder : ILiveStreamingServerBuilder
    {
        private readonly HostApplicationBuilder _builder;

        private Action<IRtmpServerConfigurator>? _configureRtmpServer;
        private Action<IServerConfigurator>? _configureServer;

        public IServiceCollection Services => _builder.Services;

        private LiveStreamingServerBuilder()
        {
            _builder = Host.CreateEmptyApplicationBuilder(default);
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
            Services.AddSingleton<IConfigurableLiveStreamingServerService, ConfigurableLiveStreamingServerService>();
            Services.AddHostedService(svc => svc.GetRequiredService<IConfigurableLiveStreamingServerService>());

            Services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);

            return new LiveStreamingServer(_builder.Build());
        }
    }
}
