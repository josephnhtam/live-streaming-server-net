using LiveStreamingServerNet.Flv.Configurations;

namespace LiveStreamingServerNet.Flv.Installer.Contracts
{
    public interface IFlvConfigurator
    {
        IFlvConfigurator ConfigureMediaMessage(Action<MediaMessageConfiguration>? configure);
    }
}
