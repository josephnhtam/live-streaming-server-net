using LiveStreamingServerNet.Rtmp.Relay.Configurations;
using LiveStreamingServerNet.Rtmp.Relay.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Relay.Installer
{
    internal class RtmpRelayConfigurator : IRtmpRelayConfigurator
    {
        public IServiceCollection Services { get; }

        public RtmpRelayConfigurator(IServiceCollection services)
        {
            Services = services;
        }

        public IRtmpRelayConfigurator ConfigureDownstream(Action<RtmpDownstreamConfiguration>? configure)
        {
            if (configure != null)
                Services.Configure(configure);

            return this;
        }
    }
}
