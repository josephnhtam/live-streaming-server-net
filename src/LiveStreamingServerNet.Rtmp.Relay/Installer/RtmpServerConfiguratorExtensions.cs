﻿using LiveStreamingServerNet.Rtmp.Relay.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal;
using LiveStreamingServerNet.Rtmp.Relay.Internal.MediaPacketDiscarders;
using LiveStreamingServerNet.Rtmp.Relay.Internal.MediaPacketDiscarders.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Services;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Streams;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Streams.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Immutable;

namespace LiveStreamingServerNet.Rtmp.Relay.Installer
{
    /// <summary>
    /// Provides extension methods for configuring RTMP relay functionality in an RTMP server.
    /// </summary>
    public static class RtmpServerConfiguratorExtensions
    {
        /// <summary>
        /// Configures RTMP relay functionality using the specified origin resolver type.
        /// </summary>
        /// <typeparam name="TRtmpOriginResolver">The type implementing IRtmpOriginResolver</typeparam>
        /// <param name="serverConfigurator">The RTMP server configurator</param>
        /// <returns>An RTMP relay configurator for additional relay settings</returns>
        public static IRtmpRelayConfigurator UseRtmpRelay<TRtmpOriginResolver>(this IRtmpServerConfigurator serverConfigurator)
            where TRtmpOriginResolver : class, IRtmpOriginResolver
        {
            var services = serverConfigurator.Services;

            AddRtmpRelay(services);
            services.TryAddSingleton<IRtmpOriginResolver, TRtmpOriginResolver>();

            return new RtmpRelayConfigurator(services);
        }

        /// <summary>
        /// Configures RTMP relay functionality using a factory method to create the origin resolver.
        /// </summary>
        /// <typeparam name="TRtmpOriginResolver">The type implementing IRtmpOriginResolver</typeparam>
        /// <param name="serverConfigurator">The RTMP server configurator</param>
        /// <param name="implementationFactory">Factory method to create the origin resolver instance</param>
        /// <returns>An RTMP relay configurator for additional relay settings</returns>
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
            services.TryAddSingleton<IRtmpRelayManager, RtmpRelayManager>();

            AddRtmpUpstreamRelay(services);
            AddRtmpDownstreamRelay(services);
        }

        private static void AddRtmpUpstreamRelay(IServiceCollection services)
        {
            services.TryAddSingleton<IRtmpUpstreamProcessFactory, RtmpUpstreamProcessFactory>();
            services.TryAddSingleton<IUpstreamMediaPacketDiscarderFactory, UpstreamMediaPacketDiscarderFactory>();

            if (!CheckService<RtmpUpstreamManagerService>(services))
            {
                services.AddSingleton<RtmpUpstreamManagerService>();

                services.AddSingleton<IRtmpServerStreamEventHandler>(
                    sp => sp.GetRequiredService<RtmpUpstreamManagerService>());

                services.AddSingleton<IRtmpMediaMessageInterceptor>(
                  sp => sp.GetRequiredService<RtmpUpstreamManagerService>());

                services.AddSingleton<IRtmpUpstreamManagerService>(
                    sp => sp.GetRequiredService<RtmpUpstreamManagerService>());
            }
        }

        private static void AddRtmpDownstreamRelay(IServiceCollection services)
        {
            services.TryAddSingleton<IRtmpDownstreamProcessFactory, RtmpDownstreamProcessFactory>();

            if (!CheckService<RtmpDownstreamManagerService>(services))
            {
                services.AddSingleton<RtmpDownstreamManagerService>();

                services.AddSingleton<IRtmpServerStreamEventHandler>(
                    sp => sp.GetRequiredService<RtmpDownstreamManagerService>());

                services.AddSingleton<IRtmpDownstreamManagerService>(
                    sp => sp.GetRequiredService<RtmpDownstreamManagerService>());
            }
        }

        private static bool CheckService<TService>(IServiceCollection services)
        {
            foreach (var service in services.ToImmutableArray())
            {
                if (service.ServiceType == typeof(TService))
                    return true;
            }

            return false;
        }
    }
}
