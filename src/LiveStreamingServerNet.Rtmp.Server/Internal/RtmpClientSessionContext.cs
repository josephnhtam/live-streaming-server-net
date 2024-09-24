using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Handshakes;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Rtmp.Server.Internal
{
    internal class RtmpClientSessionContext : IRtmpClientSessionContext
    {
        public ISessionHandle Client { get; }
        public ConcurrentDictionary<string, object> Items { get; } = new();

        public RtmpClientSessionState State { get; set; } = RtmpClientSessionState.HandshakeC0;
        public HandshakeType HandshakeType { get; set; } = HandshakeType.SimpleHandshake;

        public uint InChunkSize { get; set; } = RtmpConstants.DefaultChunkSize;
        public uint OutChunkSize { get; set; } = RtmpConstants.DefaultChunkSize;

        public uint InWindowAcknowledgementSize { get; set; }
        public uint OutWindowAcknowledgementSize { get; set; }

        public uint SequenceNumber { get; set; }
        public uint LastAcknowledgedSequenceNumber { get; set; }

        public string? AppName { get; set; }

        private uint _nextStreamId;
        private readonly ConcurrentDictionary<uint, IRtmpStream> _streams = new();
        private readonly Dictionary<uint, IRtmpChunkStreamContext> _chunkStreamContexts = new();
        private readonly IBufferPool? _bufferPool;

        public RtmpClientSessionContext(ISessionHandle client, IBufferPool? bufferPool)
        {
            Client = client;
            _bufferPool = bufferPool;
        }

        public uint GetInChunkSize()
        {
            return InChunkSize;
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

        public IRtmpStream CreateNewStream()
        {
            var streamId = Interlocked.Increment(ref _nextStreamId);

            var stream = new RtmpStream(streamId, this, _bufferPool, s => _streams.TryRemove(s.Id, out _));
            _streams[streamId] = stream;

            return stream;
        }

        public List<IRtmpStream> GetStreams()
        {
            return _streams.Values.ToList();
        }

        public IRtmpStream? GetStream(uint streamId)
        {
            return _streams.GetValueOrDefault(streamId);
        }

        public void Dispose()
        {
            foreach (var stream in _streams.Values)
                stream.Delete();
        }
    }
}
