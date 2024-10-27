using LiveStreamingServerNet.Internal.HostedServices;
using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Server.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Installer;
using LiveStreamingServerNet.Rtmp.Server.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet
{
    /// <summary>
    /// Provides extension methods for installing and configuring the live streaming server in the dependency injection container.
    /// </summary>
    public static class LiveStreamingServerInstaller
    {
        /// <summary>
        /// Adds live streaming server services to the service collection with a single endpoint.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="serverEndPoint">The server endpoint to listen on.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddLiveStreamingServer(
            this IServiceCollection services, ServerEndPoint serverEndPoint)
            => AddLiveStreamingServer(services, serverEndPoint, null, null);

        /// <summary>
        /// Adds live streaming server services to the service collection with a single endpoint.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="serverEndPoint">The server endpoint to listen on.</param>
        /// <param name="configureRtmpServer">Optional callback to configure RTMP server settings.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddLiveStreamingServer(
            this IServiceCollection services, ServerEndPoint serverEndPoint, Action<IRtmpServerConfigurator>? configureRtmpServer)
            => AddLiveStreamingServer(services, serverEndPoint, configureRtmpServer, null);

        /// <summary>
        /// Adds live streaming server services to the service collection with a single endpoint.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="serverEndPoint">The server endpoint to listen on.</param>
        /// <param name="configureRtmpServer">Optional callback to configure RTMP server settings.</param>
        /// <param name="configureServer">Optional callback to configure general server settings.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddLiveStreamingServer(
            this IServiceCollection services,
            ServerEndPoint serverEndPoint,
            Action<IRtmpServerConfigurator>? configureRtmpServer,
            Action<IServerConfigurator>? configureServer)
        {
            return AddLiveStreamingServer(services, new[] { serverEndPoint }, configureRtmpServer, configureServer);
        }

        /// <summary>
        /// Adds live streaming server services to the service collection with multiple endpoints.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="serverEndPoints">Collection of server endpoints to listen on.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddLiveStreamingServer(
            this IServiceCollection services, IEnumerable<ServerEndPoint> serverEndPoints)
            => AddLiveStreamingServer(services, serverEndPoints, null, null);

        /// <summary>
        /// Adds live streaming server services to the service collection with multiple endpoints.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="serverEndPoints">Collection of server endpoints to listen on.</param>
        /// <param name="configureRtmpServer">Optional callback to configure RTMP server settings.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddLiveStreamingServer(
            this IServiceCollection services, IEnumerable<ServerEndPoint> serverEndPoints, Action<IRtmpServerConfigurator>? configureRtmpServer)
            => AddLiveStreamingServer(services, serverEndPoints, configureRtmpServer, null);

        /// <summary>
        /// Adds live streaming server services to the service collection with multiple endpoints.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="serverEndPoints">Collection of server endpoints to listen on.</param>
        /// <param name="configureRtmpServer">Optional callback to configure RTMP server settings.</param>
        /// <param name="configureServer">Optional callback to configure general server settings.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddLiveStreamingServer(
            this IServiceCollection services,
            IEnumerable<ServerEndPoint> serverEndPoints,
            Action<IRtmpServerConfigurator>? configureRtmpServer,
            Action<IServerConfigurator>? configureServer)
        {
            if (!serverEndPoints.Any())
                throw new ArgumentException("At least one server endpoint must be provided.", nameof(serverEndPoints));

            services.AddRtmpServer(configureRtmpServer, configureServer);

            services.AddHostedService(svc =>
                ActivatorUtilities.CreateInstance<LiveStreamingServerService>(svc, serverEndPoints.ToList()));

            return services;
        }
    }
}
