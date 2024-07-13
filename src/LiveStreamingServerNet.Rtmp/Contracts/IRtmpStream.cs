using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Contracts
{
    public interface IRtmpStream
    {
        string StreamPath { get; }
        IReadOnlyDictionary<string, string> StreamArguments { get; }
        IReadOnlyDictionary<string, object>? MetaData { get; }

        DateTime StartTime { get; }
        IClientControl Publisher { get; }
        IReadOnlyList<IClientControl> Subscribers { get; }
    }
}
