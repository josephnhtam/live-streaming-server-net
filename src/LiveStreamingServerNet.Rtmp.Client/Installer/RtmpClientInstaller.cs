using LiveStreamingServerNet.Networking.Client.Contracts;
using LiveStreamingServerNet.Networking.Client.Installer;
using LiveStreamingServerNet.Networking.Client.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Installer.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Mediators;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Client.Installer
{
    /// <summary>
    /// Static class providing extension methods for registering RTMP client services with dependency injection.
    /// </summary>
    public static class RtmpClientInstaller
    {
        /// <summary>
        /// Adds RTMP client services to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddRtmpClient(this IServiceCollection services)
            => services.AddRtmpClient(null, null);

        /// <summary>
        /// Adds RTMP client services to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <param name="configureRtmpClient">Action to configure RTMP client settings</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddRtmpClient(
            this IServiceCollection services, Action<IRtmpClientConfigurator>? configureRtmpClient)
            => services.AddRtmpClient(configureRtmpClient, null);

        /// <summary>
        /// Adds RTMP client services to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <param name="configureRtmpClient">Action to configure RTMP client settings</param>
        /// <param name="configureClient">Action to configure general client settings</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddRtmpClient(
            this IServiceCollection services,
            Action<IRtmpClientConfigurator>? configureRtmpClient,
            Action<IClientConfigurator>? configureClient)
        {
            services.AddRtmpCore(configureClient)
                    .AddRtmpServices()
                    .AddRtmpMessageHandlers()
                    .AddRtmpCommandHandlers()
                    .AddRtmpClientEventDispatchers()
                    .AddRtmpClientEventHandlers();

            configureRtmpClient?.Invoke(new RtmpClientConfigurator(services));

            return services;
        }

        private static IServiceCollection AddRtmpCore(this IServiceCollection services, Action<IClientConfigurator>? configureClient)
        {
            services.AddClient<RtmpSessionHandlerFactory>(options => configureClient?.Invoke(options));

            services.AddMediator(options => options.AddRequestHandlerFromAssembly(typeof(RtmpClientInstaller).Assembly));

            services.AddSingleton<IRtmpClientContext, RtmpClientContext>()
                    .AddSingleton<IRtmpSessionContextFactory, RtmpSessionContextFactory>();

            services.AddSingleton<RtmpClient>()
                    .AddSingleton<IRtmpClient>(sp => sp.GetRequiredService<RtmpClient>());

            services.AddSingleton<IRtmpStreamFactory, RtmpStreamFactory>();

            return services;
        }

        private static IServiceCollection AddRtmpServices(this IServiceCollection services)
        {
            services.AddSingleton<IRtmpChunkMessageAggregatorService, RtmpChunkMessageAggregatorService>()
                    .AddSingleton<IRtmpChunkMessageWriterService, RtmpChunkMessageWriterService>();

            services.AddSingleton<IRtmpChunkMessageSenderService, RtmpChunkMessageSenderService>()
                    .AddSingleton<IRtmpProtocolControlService, RtmpProtocolControlService>()
                    .AddSingleton<IRtmpCommandMessageSenderService, RtmpCommandMessageSenderService>()
                    .AddSingleton<IRtmpCommanderService, RtmpCommanderService>()
                    .AddSingleton<IRtmpMediaDataSenderService, RtmpMediaDataSenderService>()
                    .AddSingleton<IRtmpAcknowledgementHandlerService, RtmpAcknowledgementHandlerService>();

            services.AddSingleton<RtmpCommandResultManagerService>()
                    .AddSingleton<IRtmpCommandResultManagerService>(sp => sp.GetRequiredService<RtmpCommandResultManagerService>());

            return services;
        }

        private static IServiceCollection AddRtmpMessageHandlers(this IServiceCollection services)
        {
            return services.AddRtmpMessageHandlers<IRtmpSessionContext>(typeof(RtmpClientInstaller).Assembly);
        }

        private static IServiceCollection AddRtmpCommandHandlers(this IServiceCollection services)
        {
            return services.AddRtmpCommandHandlers<IRtmpSessionContext>(typeof(RtmpClientInstaller).Assembly);
        }

        private static IServiceCollection AddRtmpClientEventDispatchers(this IServiceCollection services)
        {
            services.AddSingleton<IRtmpHandshakeEventDispatcher, RtmpHandshakeEventDispatcher>();

            return services;
        }

        private static IServiceCollection AddRtmpClientEventHandlers(this IServiceCollection services)
        {
            services.AddSingleton<IRtmpHandshakeEventHandler>(sp => sp.GetRequiredService<RtmpClient>())
                    .AddSingleton<IClientEventHandler>(sp => sp.GetRequiredService<RtmpCommandResultManagerService>());

            return services;
        }
    }
}