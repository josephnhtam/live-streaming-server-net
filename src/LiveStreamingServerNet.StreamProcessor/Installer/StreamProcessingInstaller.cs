using LiveStreamingServerNet.Rtmp.Installer.Contracts;
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
    public static class StreamProcessingInstaller
    {
        public static IStreamProcessingBuilder AddStreamProcessor(
            this IRtmpServerConfigurator rtmpServerConfigurator,
            Action<IStreamProcessingConfigurator>? configure = null)
        {
            var services = rtmpServerConfigurator.Services;

            services.AddSingleton<IStreamProcessorEventDispatcher, StreamProcessorEventDispatcher>()
                    .AddSingleton<IStreamProcessorManager, StreamProcessorManager>();

            rtmpServerConfigurator.AddStreamEventHandler<RtmpServerStreamEventListener>();

            configure?.Invoke(new StreamProcessorConfigurator(services));

            services.TryAddSingleton<IInputPathResolver, DefaultInputPathResolver>();

            return new StreamProcessorBuilder(services);
        }
    }

    public class StreamProcessorBuilder : IStreamProcessingBuilder
    {
        public IServiceCollection Services { get; }

        public StreamProcessorBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
