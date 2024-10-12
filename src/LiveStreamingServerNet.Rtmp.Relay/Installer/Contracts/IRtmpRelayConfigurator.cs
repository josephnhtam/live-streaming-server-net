using LiveStreamingServerNet.Rtmp.Relay.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Relay.Installer.Contracts
{
    public interface IRtmpRelayConfigurator
    {
        IServiceCollection Services { get; }
        IRtmpRelayConfigurator ConfigureUpstream(Action<RtmpUpstreamConfiguration>? configure);
        IRtmpRelayConfigurator ConfigureDownstream(Action<RtmpDownstreamConfiguration>? configure);
    }
}
