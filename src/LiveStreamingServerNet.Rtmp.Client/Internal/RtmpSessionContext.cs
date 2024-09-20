using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal class RtmpSessionContext : IRtmpSessionContext
    {
        public ISessionHandle Session { get; }
        public ConcurrentDictionary<string, object> Items { get; } = new();

        public RtmpSessionState State { get; set; } = RtmpSessionState.HandshakeS0;

        public uint InChunkSize { get; set; } = RtmpConstants.DefaultChunkSize;
        public uint OutChunkSize { get; set; } = RtmpConstants.DefaultChunkSize;

        private readonly Dictionary<uint, IRtmpChunkStreamContext> _chunkStreamContexts = new();

        public RtmpSessionContext(ISessionHandle session)
        {
            Session = session;
        }

        public IRtmpChunkStreamContext GetChunkStreamContext(uint chunkStreamId)
        {
            lock (_chunkStreamContexts)
            {
                if (_chunkStreamContexts.TryGetValue(chunkStreamId, out var context))
                    return context;

                return _chunkStreamContexts[chunkStreamId] = new RtmpChunkStreamContext(chunkStreamId);
            }
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}