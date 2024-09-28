using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal class RtmpStreamContext : IRtmpStreamContext
    {
        public uint StreamId { get; }

        public IRtmpSessionContext SessionContext { get; }

        public RtmpStreamContext(uint streamId, IRtmpSessionContext sessionContext)
        {
            StreamId = streamId;
            SessionContext = sessionContext;
        }

        public void Dispose()
        {
        }
    }
}
