using LiveStreamingServerNet.Rtmp.Server.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Installer.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal;
using LiveStreamingServerNet.StreamProcessor.Internal.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Services;
using LiveStreamingServerNet.StreamProcessor.Internal.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.StreamProcessor.Installer
{
    /// <summary>
    /// Provides extension methods for installing and configuring stream processing services.
    /// </summary>
    public static class StreamProcessingInstaller
    {
        /// <summary>
        /// Adds stream processing to the RTMP server.
        /// </summary>
        /// <param name="rtmpServerConfigurator">The RTMP server configurator to add services to.</param>
        /// <returns>A stream processing builder for further configuration.</returns>
        public static IStreamProcessingBuilder AddStreamProcessor(this IRtmpServerConfigurator rtmpServerConfigurator)
            => AddStreamProcessor(rtmpServerConfigurator, null);

        /// <summary>
        /// Adds stream processing to the RTMP server.
        /// </summary>
        /// <param name="rtmpServerConfigurator">The RTMP server configurator to add services to.</param>
        /// <param name="configure">Optional action to configure additional stream processing settings.</param>
        /// <returns>A stream processing builder for further configuration.</returns>
        public static IStreamProcessingBuilder AddStreamProcessor(
            this IRtmpServerConfigurator rtmpServerConfigurator,
            Action<IStreamProcessingConfigurator>? configure)
        {
            var services = rtmpServerConfigurator.Services;

            services.AddSingleton<IStreamProcessorEventDispatcher, StreamProcessorEventDispatcher>()
                    .AddSingleton<IStreamProcessorManager, StreamProcessorManager>();

            rtmpServerConfigurator.AddStreamEventHandler<RtmpServerStreamEventListener>();

            configure?.Invoke(new StreamProcessingConfigurator(services));

            services.TryAddSingleton<IInputPathResolver, DefaultInputPathResolver>();

            return new StreamProcessingBuilder(services);
        }
    }

    internal class StreamProcessingBuilder : IStreamProcessingBuilder
    {
        public IServiceCollection Services { get; }

        public StreamProcessingBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
