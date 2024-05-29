using LiveStreamingServerNet.StreamProcessor.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.StreamProcessor.Installer.Contracts
{
    public interface IStreamProcessingConfigurator
    {
        IServiceCollection Services { get; }

        IStreamProcessingConfigurator UseInputPathResolver<TInputPathResolver>()
            where TInputPathResolver : class, IInputPathResolver;
        IStreamProcessingConfigurator UseInputPathResolver(Func<IServiceProvider, IInputPathResolver> implementationFactory);

        IStreamProcessingConfigurator AddStreamProcessorEventHandler<TStreamProcessorEventHandler>()
            where TStreamProcessorEventHandler : class, IStreamProcessorEventHandler;
        IStreamProcessingConfigurator AddStreamProcessorEventHandler(Func<IServiceProvider, IStreamProcessorEventHandler> implementationFactory);
    }
}
