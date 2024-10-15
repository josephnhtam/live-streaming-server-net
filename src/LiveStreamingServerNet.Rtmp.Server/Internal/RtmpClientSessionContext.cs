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

        private uint _lastStreamId = RtmpConstants.ReservedStreamId;
        private uint _lastChunkStreamId = RtmpConstants.ReservedChunkStreamId;

        private readonly ConcurrentDictionary<uint, IRtmpStreamContext> _streamContexts = new();
        private readonly ConcurrentDictionary<uint, IRtmpChunkStreamContext> _chunkStreamContexts = new();
        private readonly IBufferPool? _bufferPool;

        public RtmpClientSessionContext(ISessionHandle client, IBufferPool? bufferPool)
        {
            Client = client;
            _bufferPool = bufferPool;
        }

        public IRtmpChunkStreamContext GetChunkStreamContext(uint chunkStreamId)
        {
            return _chunkStreamContexts.GetOrAdd(chunkStreamId, CreateChunkStreamContext);

            static IRtmpChunkStreamContext CreateChunkStreamContext(uint chunkStreamId)
                => new RtmpChunkStreamContext(chunkStreamId);
        }

        public uint GetNextChunkStreamId()
        {
            return Interlocked.Increment(ref _lastChunkStreamId);
        }

        public IRtmpStreamContext CreateStreamContext()
        {
            var streamId = Interlocked.Increment(ref _lastStreamId);

            var streamContext = new RtmpStreamContext(streamId, this, _bufferPool);
            _streamContexts[streamId] = streamContext;

            return streamContext;
        }

        public List<IRtmpStreamContext> GetStreamContexts()
        {
            return _streamContexts.Values.ToList();
        }

        public IRtmpStreamContext? GetStreamContext(uint streamId)
        {
            return _streamContexts.GetValueOrDefault(streamId);
        }

        public void RemoveStreamContext(uint streamId)
        {
            if (_streamContexts.TryRemove(streamId, out var streamContext))
                streamContext.Dispose();
        }

        public void Recycle(IDataBufferPool dataBufferPool)
        {
            foreach (var chunkStreamContext in _chunkStreamContexts.Values)
                chunkStreamContext.RecyclePayload(dataBufferPool);
        }

        public ValueTask DisposeAsync()
        {
            foreach (var streamContext in _streamContexts.Values)
                RemoveStreamContext(streamContext.StreamId);

            return ValueTask.CompletedTask;
        }
    }
}
