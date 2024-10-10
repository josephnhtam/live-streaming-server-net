using LiveStreamingServerNet.Rtmp.Relay.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Services;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Immutable;

namespace LiveStreamingServerNet.Rtmp.Relay.Installer
{
    public static class RtmpServerConfiguratorExtensions
    {

        public static IRtmpRelayConfigurator UseRtmpRelay<TRtmpOriginResolver>(this IRtmpServerConfigurator serverConfigurator)
            where TRtmpOriginResolver : class, IRtmpOriginResolver
        {
            var services = serverConfigurator.Services;

            AddRtmpRelay(services);
            services.TryAddSingleton<IRtmpOriginResolver, TRtmpOriginResolver>();

            return new RtmpRelayConfigurator(services);
        }

        public static IRtmpRelayConfigurator UseRtmpRelay<TRtmpOriginResolver>(
            this IRtmpServerConfigurator serverConfigurator, Func<IServiceProvider, TRtmpOriginResolver> implementationFactory)
        {
            var services = serverConfigurator.Services;

            AddRtmpRelay(services);
            services.TryAddSingleton(implementationFactory);

            return new RtmpRelayConfigurator(services);
        }

        private static void AddRtmpRelay(IServiceCollection services)
        {
            services.TryAddSingleton<IRtmpDownstreamProcessFactory, RtmpDownstreamProcessFactory>();

            foreach (var service in services.ToImmutableArray())
            {
                if (service.ServiceType == typeof(RtmpDownstreamManagerService))
                    break;

                services.AddSingleton<RtmpDownstreamManagerService>();

                services.AddSingleton<IRtmpServerStreamEventHandler>(
                    sp => sp.GetRequiredService<RtmpDownstreamManagerService>());

                services.AddSingleton<IRtmpDownstreamManagerService>(
                    sp => sp.GetRequiredService<RtmpDownstreamManagerService>());
            }
        }
    }
}
