using LiveStreamingServerNet.Rtmp.Client.Configurations;
using LiveStreamingServerNet.Rtmp.Client.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Client.Installer
{
    internal class RtmpClientConfigurator : IRtmpClientConfigurator
    {
        public IServiceCollection Services { get; }

        public RtmpClientConfigurator(IServiceCollection services)
        {
            Services = services;
        }

        public IRtmpClientConfigurator Configure(Action<RtmpClientConfiguration>? configure)
        {
            if (configure != null)
                Services.Configure(configure);
            return this;
        }
    }
}