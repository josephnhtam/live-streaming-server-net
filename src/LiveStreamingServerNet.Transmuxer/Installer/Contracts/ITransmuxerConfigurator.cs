using LiveStreamingServerNet.Transmuxer.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Transmuxer.Installer.Contracts
{
    public interface ITransmuxerConfigurator
    {
        IServiceCollection Services { get; }
        ITransmuxerConfigurator UseInputPathResolver<TInputPathResolver>() where TInputPathResolver : class, IInputPathResolver;
        ITransmuxerConfigurator UserOutputDirectoryPathResolver<TOutputDirectoryPathResolver>() where TOutputDirectoryPathResolver : class, IOutputDirectoryPathResolver;
    }
}
