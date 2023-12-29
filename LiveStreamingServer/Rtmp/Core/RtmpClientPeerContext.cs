using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.Utilities;
using System.Collections.Concurrent;

namespace LiveStreamingServer.Rtmp.Core
{
    public class RtmpClientPeerContext : IRtmpClientPeerContext
    {
        public IClientPeerHandle Peer { get; }
        public RtmpClientPeerState State { get; set; } = RtmpClientPeerState.HandshakeC0;
        public HandshakeType HandshakeType { get; set; } = HandshakeType.SimpleHandshake;

        public uint InChunkSize { get; set; } = RtmpConstants.DefaultChunkSize;
        public uint OutChunkSize { get; set; } = RtmpConstants.DefaultChunkSize;

        public string AppName { get; set; } = default!;
        public string PublishStreamPath { get; set; } = default!;
        public IDictionary<string, string> PublishStreamArguments { get; set; } = new Dictionary<string, string>();

        public uint PublishStreamId => _streamId;

        private uint _streamId;
        private ConcurrentDictionary<uint, IRtmpChunkStreamContext> _chunkStreamContexts = new();

        public RtmpClientPeerContext(IClientPeerHandle peer)
        {
            Peer = peer;
        }

        public uint NextPublishStreamId()
        {
            return Interlocked.Add(ref _streamId, 1);
        }

        public IRtmpChunkStreamContext GetChunkStreamContext(uint chunkStreamId)
        {
            return _chunkStreamContexts.GetOrAdd(chunkStreamId, new RtmpChunkStreamContext(chunkStreamId));
        }
    }
}
