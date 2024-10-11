using LiveStreamingServerNet.Flv.Configurations;

namespace LiveStreamingServerNet.Flv.Installer.Contracts
{
    public interface IFlvConfigurator
    {
        IFlvConfigurator ConfigureMediaPacket(Action<MediaPacketConfiguration>? configure);
    }
}
