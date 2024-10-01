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

        public uint InWindowAcknowledgementSize { get; set; }
        public uint OutWindowAcknowledgementSize { get; set; }

        public uint SequenceNumber { get; set; }
        public uint LastAcknowledgedSequenceNumber { get; set; }

        public string? AppName { get; set; }

        private uint _lastChunkStreamId = RtmpConstants.ReservedChunkStreamId;

        private readonly ConcurrentDictionary<uint, IRtmpStreamContext> _streamContexts = new();
        private readonly ConcurrentDictionary<uint, IRtmpChunkStreamContext> _chunkStreamContexts = new();

        public RtmpSessionContext(ISessionHandle session)
        {
            Session = session;
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

        public IRtmpStreamContext CreateStreamContext(uint streamId)
        {
            if (_streamContexts.ContainsKey(streamId))
                throw new InvalidOperationException($"Stream context with ID {streamId} already exists.");

            var streamContext = new RtmpStreamContext(streamId, this);
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

        public ValueTask DisposeAsync()
        {
            foreach (var streamContext in _streamContexts.Values)
                RemoveStreamContext(streamContext.StreamId);

            return ValueTask.CompletedTask;
        }
    }
}