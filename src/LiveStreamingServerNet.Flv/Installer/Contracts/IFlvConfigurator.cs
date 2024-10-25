using LiveStreamingServerNet.Flv.Configurations;

namespace LiveStreamingServerNet.Flv.Installer.Contracts
{
    public interface IFlvConfigurator
    {
        IFlvConfigurator Configure(Action<FlvConfiguration>? configure);
        IFlvConfigurator ConfigureMediaStreaming(Action<MediaStreamingConfiguration>? configure);
    }
}
