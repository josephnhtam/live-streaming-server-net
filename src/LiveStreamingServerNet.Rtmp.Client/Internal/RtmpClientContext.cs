using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal class RtmpClientContext : IRtmpClientContext
    {
        public IRtmpSessionContext? SessionContext { get; set; }
    }
}
