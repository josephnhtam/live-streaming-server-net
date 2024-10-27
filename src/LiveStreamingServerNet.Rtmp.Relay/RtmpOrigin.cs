using LiveStreamingServerNet.Networking;

namespace LiveStreamingServerNet.Rtmp.Relay
{
    /// <summary>
    /// Represents an RTMP origin server and the target stream details.
    /// </summary>
    /// <param name="EndPoint">The endpoint of the origin server</param>
    /// <param name="AppName">The RTMP application name on the origin server (first part of RTMP URL path)</param>
    /// <param name="StreamName">The stream key or name on the origin server (second part of RTMP URL path)</param>
    /// <remarks>
    /// The complete RTMP URL would be: rtmp://[EndPoint]/[AppName]/[StreamName]
    /// </remarks>
    public record RtmpOrigin(ServerEndPoint EndPoint, string AppName, string StreamName);
}
