using LiveStreamingServerNet.Internal.HostedServices;
using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Installer;
using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet
{
    public static class LiveStreamingServerInstaller
    {
        public static IServiceCollection AddLiveStreamingServer(
            this IServiceCollection services,
            IEnumerable<ServerEndPoint> serverEndPoints,
            Action<IRtmpServerConfigurator>? configureRtmpServer = null,
            Action<IServerConfigurator>? configureServer = null)
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
