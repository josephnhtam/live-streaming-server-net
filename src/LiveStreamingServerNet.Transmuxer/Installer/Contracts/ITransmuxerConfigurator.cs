using LiveStreamingServerNet.Transmuxer.Configurations;
using LiveStreamingServerNet.Transmuxer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Transmuxer.Installer.Contracts
{
    public interface ITransmuxerConfigurator
    {
        IServiceCollection Services { get; }
        ITransmuxerConfigurator Configure(Action<RemuxingConfiguration>? configure);
        ITransmuxerConfigurator UseInputPathResolver<TInputPathResolver>() where TInputPathResolver : class, IInputPathResolver;
        ITransmuxerConfigurator UserOutputDirectoryPathResolver<TOutputDirectoryPathResolver>() where TOutputDirectoryPathResolver : class, IOutputDirectoryPathResolver;
    }
}
