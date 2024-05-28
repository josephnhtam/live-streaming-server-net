using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.StreamProcessor.Installer.Contracts
{
    public interface IStreamProcessingBuilder
    {
        IServiceCollection Services { get; }
    }
}
