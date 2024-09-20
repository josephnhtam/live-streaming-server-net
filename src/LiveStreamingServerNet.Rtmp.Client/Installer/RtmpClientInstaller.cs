using LiveStreamingServerNet.Networking.Client.Installer;
using LiveStreamingServerNet.Networking.Client.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Client.Installer
{
    public static class RtmpClientInstaller
    {
        public static IServiceCollection AddRtmpClient(
            this IServiceCollection services,
            Action<IRtmpClientConfigurator>? configureRtmpClient = null,
            Action<IClientConfigurator>? configureClient = null)
        {
            services.AddRtmpCore(configureClient);
            //.AddRtmpServices()
            //.AddRtmpMessageHandlers()
            //.AddRtmpCommandHandlers()
            //.AddRtmpServerEventDispatchers()
            //.AddRtmpServerEventHandlers();

            configureRtmpClient?.Invoke(new RtmpClientConfigurator(services));

            //services.AddDefaults();

            return services;
        }

        private static IServiceCollection AddRtmpCore(this IServiceCollection services, Action<IClientConfigurator>? configureClient)
        {
            services.AddClient<RtmpSessionHandlerFactory>(options => configureClient?.Invoke(options));

            services.AddMediator();

            services.AddSingleton<IRtmpSessionContextFactory, RtmpSessionContextFactory>();

            return services;
        }
    }
}