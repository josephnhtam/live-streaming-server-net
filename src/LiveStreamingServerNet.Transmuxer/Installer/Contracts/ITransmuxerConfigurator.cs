using LiveStreamingServerNet.Transmuxer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Transmuxer.Installer.Contracts
{
    public interface ITransmuxerConfigurator
    {
        IServiceCollection Services { get; }

        ITransmuxerConfigurator UseInputPathResolver<TInputPathResolver>()
            where TInputPathResolver : class, IInputPathResolver;
        ITransmuxerConfigurator UseInputPathResolver(Func<IServiceProvider, IInputPathResolver> implmentationFactory);

        ITransmuxerConfigurator AddTransmuxerEventHandler<TTransmuxerEventHandler>()
            where TTransmuxerEventHandler : class, ITransmuxerEventHandler;
        ITransmuxerConfigurator AddTransmuxerEventHandler(Func<IServiceProvider, ITransmuxerEventHandler> implmentationFactory);
    }
}
