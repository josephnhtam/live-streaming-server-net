using LiveStreamingServerNet.Transmuxer.Hls.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Transmuxer.Installer.Contracts
{
    public interface IHlsUploaderConfigurator
    {
        IServiceCollection Services { get; }

        IHlsUploaderConfigurator AddHlsStorageEventHandler<THlsStorageEventHandler>()
            where THlsStorageEventHandler : class, IHlsStorageEventHandler;
        IHlsUploaderConfigurator AddTransmuxerEventHandler(Func<IServiceProvider, IHlsStorageEventHandler> implmentationFactory);
    }
}
