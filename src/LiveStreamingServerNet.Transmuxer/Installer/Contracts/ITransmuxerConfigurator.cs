using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Transmuxer.Configurations;
using LiveStreamingServerNet.Transmuxer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Transmuxer.Installer.Contracts
{
    public interface ITransmuxerConfigurator
    {
        IServiceCollection Services { get; }
        ITransmuxerConfigurator Configure(Action<RemuxingConfiguration>? configure);

        ITransmuxerConfigurator UseInputPathResolver<TInputPathResolver>()
            where TInputPathResolver : class, IInputPathResolver;
        ITransmuxerConfigurator UseInputPathResolver(Func<IServiceProvider, IInputPathResolver> implmentationFactory);

        ITransmuxerConfigurator UserOutputDirectoryPathResolver<TOutputDirectoryPathResolver>()
            where TOutputDirectoryPathResolver : class, IOutputDirectoryPathResolver;
        ITransmuxerConfigurator UserOutputDirectoryPathResolver(Func<IServiceProvider, IOutputDirectoryPathResolver> implmentationFactory);

        ITransmuxerConfigurator AddTransmuxerEventHandler<TTransmuxerEventHandler>()
            where TTransmuxerEventHandler : class, ITransmuxerEventHandler;
        ITransmuxerConfigurator AddTransmuxerEventHandler(Func<IServiceProvider, ITransmuxerEventHandler> implmentationFactory);
    }
}
