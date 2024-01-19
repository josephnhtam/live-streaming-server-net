using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Transmuxer.Installer.Contracts
{
    public interface ITransmuxerBuilder
    {
        IServiceCollection Services { get; }
    }
}
