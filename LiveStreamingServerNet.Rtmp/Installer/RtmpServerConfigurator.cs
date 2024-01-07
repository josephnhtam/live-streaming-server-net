using LiveStreamingServerNet.Rtmp.Configurations;
using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Installer
{
    public class RtmpServerConfigurator : IRtmpServerConfigurator
    {
        public IServiceCollection Services { get; }

        public RtmpServerConfigurator(IServiceCollection services)
        {
            Services = services;
        }

        public IRtmpServerConfigurator ConfigureRtmpServer(Action<RtmpServerConfiguration>? configure)
        {
            if (configure != null)
                Services.Configure(configure);

            return this;
        }

        public IRtmpServerConfigurator ConfigureMediaMessage(Action<MediaMessageConfiguration>? configure)
        {
            if (configure != null)
                Services.Configure(configure);

            return this;
        }
    }
}
