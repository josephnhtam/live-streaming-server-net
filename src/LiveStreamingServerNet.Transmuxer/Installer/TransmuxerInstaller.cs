using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Installer.Contracts;
using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Installer.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal;
using LiveStreamingServerNet.Transmuxer.Internal.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Services;
using LiveStreamingServerNet.Transmuxer.Internal.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LiveStreamingServerNet.Transmuxer.Installer
{
    public static class TransmuxerInstaller
    {
        public static ITransmuxerBuilder AddTransmuxer(
            this IRtmpServerConfigurator rtmpServerConfigurator,
            Action<ITransmuxerConfigurator>? configure = null)
        {
            var services = rtmpServerConfigurator.Services;

            services.AddSingleton<ITransmuxerEventDispatcher, TransmuxerEventDispatcher>()
                    .AddSingleton<ITransmuxerManager, TransmuxerManager>();

            services.AddSingleton<IServerEventHandler, ServerEventHandler>();
            rtmpServerConfigurator.AddStreamEventHandler<RtmpServerStreamEventListener>();

            configure?.Invoke(new TransmuxerConfigurator(services));

            services.TryAddSingleton<IInputPathResolver, DefaultInputPathResolver>();

            return new TransmuxerBuilder(services);
        }
    }

    public class TransmuxerBuilder : ITransmuxerBuilder
    {
        public IServiceCollection Services { get; }

        public TransmuxerBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
