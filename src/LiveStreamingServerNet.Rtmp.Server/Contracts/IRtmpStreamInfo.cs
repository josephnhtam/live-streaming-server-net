using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Contracts
{
    public interface IRtmpStreamInfo
    {
        string StreamPath { get; }
        IReadOnlyDictionary<string, string> StreamArguments { get; }
        IReadOnlyDictionary<string, object>? MetaData { get; }

        DateTime StartTime { get; }
        ISessionControl Publisher { get; }
        IReadOnlyList<ISessionControl> Subscribers { get; }
    }
}
