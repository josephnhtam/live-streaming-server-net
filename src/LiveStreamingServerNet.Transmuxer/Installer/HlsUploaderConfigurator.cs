using LiveStreamingServerNet.Transmuxer.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Installer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Transmuxer.Installer
{
    internal class HlsUploaderConfigurator : IHlsUploaderConfigurator
    {
        public IServiceCollection Services { get; }

        public HlsUploaderConfigurator(IServiceCollection services)
        {
            Services = services;
        }

        public IHlsUploaderConfigurator AddHlsStorageEventHandler<THlsStorageEventHandler>()
            where THlsStorageEventHandler : class, IHlsStorageEventHandler
        {
            Services.AddSingleton<IHlsStorageEventHandler, THlsStorageEventHandler>();
            return this;
        }

        public IHlsUploaderConfigurator AddTransmuxerEventHandler(Func<IServiceProvider, IHlsStorageEventHandler> implmentationFactory)
        {
            Services.AddSingleton(implmentationFactory);
            return this;
        }
    }
}
