using LiveStreamingServerNet.WebRTC.Utilities;

namespace LiveStreamingServerNet.WebRTC.Ice.Contracts
{
    public interface ILocalPreferenceScoreProvider
    {
        ushort ProvideLocalPreferenceScore(LocalIPAddressInfo localAddress);
    }
}
