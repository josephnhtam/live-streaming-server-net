using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.StreamProcessor.Installer.Contracts
{
    public interface IHlsUploaderConfigurator
    {
        IServiceCollection Services { get; }

        IHlsUploaderConfigurator AddHlsStorageEventHandler<THlsStorageEventHandler>()
            where THlsStorageEventHandler : class, IHlsStorageEventHandler;
        IHlsUploaderConfigurator AddHlsStorageEventHandler(Func<IServiceProvider, IHlsStorageEventHandler> implementationFactory);

        IHlsUploaderConfigurator AddHlsUploaderCondition<THlsUploaderCondition>()
            where THlsUploaderCondition : class, IHlsUploaderCondition;
        IHlsUploaderConfigurator AddHlsUploaderCondition(Func<IServiceProvider, IHlsUploaderCondition> implementationFactory);
    }
}
