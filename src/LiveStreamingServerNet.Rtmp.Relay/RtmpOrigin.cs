using LiveStreamingServerNet.Networking;

namespace LiveStreamingServerNet.Rtmp.Relay
{
    public record RtmpOrigin(ServerEndPoint EndPoint, string AppName, string StreamName);
}
