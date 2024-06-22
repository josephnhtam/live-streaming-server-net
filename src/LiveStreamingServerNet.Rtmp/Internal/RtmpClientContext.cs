using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Handshakes;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal
{
    internal class RtmpClientContext : IRtmpClientContext
    {
        public IClientHandle Client { get; }
        public RtmpClientState State { get; set; } = RtmpClientState.HandshakeC0;
        public HandshakeType HandshakeType { get; set; } = HandshakeType.SimpleHandshake;

        public uint InChunkSize { get; set; } = RtmpConstants.DefaultChunkSize;
        public uint OutChunkSize { get; set; } = RtmpConstants.DefaultChunkSize;

        public uint InWindowAcknowledgementSize { get; set; }
        public uint OutWindowAcknowledgementSize { get; set; }

        public uint SequenceNumber { get; set; }
        public uint LastAcknowledgedSequenceNumber { get; set; }

        public string AppName { get; set; } = default!;
        public uint? StreamId => _isStreamCreated ? _streamId : null;

        public uint VideoTimestamp => _videoTimestamp;
        public uint AudioTimestamp => _audioTimestamp;

        public IRtmpPublishStreamContext? PublishStreamContext { get; private set; }
        public IRtmpStreamSubscriptionContext? StreamSubscriptionContext { get; private set; }

        private uint _streamId;
        private bool _isStreamCreated;
        private uint _videoTimestamp;
        private uint _audioTimestamp;
        private readonly object _videoTimestampSyncLock = new();
        private readonly object _audioTimestampSyncLock = new();
        private readonly Dictionary<uint, IRtmpChunkStreamContext> _chunkStreamContexts = new();
        private readonly IBufferPool? _bufferPool;

        public RtmpClientContext(IClientHandle client, IBufferPool? bufferPool)
        {
            Client = client;
            _bufferPool = bufferPool;
            State = RtmpClientState.HandshakeC0;
        }

        public IRtmpPublishStreamContext CreatePublishStreamContext(string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var newContext = new RtmpPublishStreamContext(_streamId, streamPath, streamArguments, _bufferPool);
            PublishStreamContext = newContext;
            return newContext;
        }

        public IRtmpStreamSubscriptionContext CreateStreamSubscriptionContext(uint chunkStreamId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var newContext = new RtmpStreamSubscriptionContext(_streamId, chunkStreamId, streamPath, streamArguments);
            StreamSubscriptionContext = newContext;
            return newContext;
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

        public uint CreateNewStream()
        {
            var streamId = Interlocked.Increment(ref _streamId);
            _isStreamCreated = true;
            return streamId;
        }

        public void DeleteStream()
        {
            _isStreamCreated = false;
            PublishStreamContext = null;
            StreamSubscriptionContext = null;
        }

        public bool UpdateTimestamp(uint timestamp, MediaType mediaType)
        {
            switch (mediaType)
            {
                case MediaType.Audio:
                    lock (_videoTimestampSyncLock)
                    {
                        if (timestamp > _videoTimestamp)
                        {
                            _videoTimestamp = timestamp;
                            return true;
                        }

                        return false;
                    }

                default:
                    lock (_audioTimestampSyncLock)
                    {
                        if (timestamp > _audioTimestamp)
                        {
                            _audioTimestamp = timestamp;
                            return true;
                        }

                        return false;
                    }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (PublishStreamContext != null)
                await PublishStreamContext.DisposeAsync();

            if (StreamSubscriptionContext != null)
                await StreamSubscriptionContext.DisposeAsync();
        }
    }

    internal class RtmpPublishStreamContext : IRtmpPublishStreamContext
    {
        public uint StreamId { get; }
        public string StreamPath { get; }
        public IReadOnlyDictionary<string, string> StreamArguments { get; }
        public IReadOnlyDictionary<string, object>? StreamMetaData { get; set; }
        public byte[]? VideoSequenceHeader { get; set; }
        public byte[]? AudioSequenceHeader { get; set; }
        public bool GroupOfPicturesCacheActivated { get; set; }
        public IGroupOfPicturesCache GroupOfPicturesCache { get; }

        public RtmpPublishStreamContext(uint streamId, string streamPath, IReadOnlyDictionary<string, string> streamArguments, IBufferPool? bufferPool)
        {
            StreamId = streamId;
            StreamPath = streamPath;
            StreamArguments = new Dictionary<string, string>(streamArguments);
            GroupOfPicturesCache = new GroupOfPicturesCache(bufferPool);
        }

        public ValueTask DisposeAsync()
        {
            GroupOfPicturesCache.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    internal class GroupOfPicturesCache : IGroupOfPicturesCache
    {
        private readonly IBufferCache<PictureCacheInfo> _cache;
        public long Size => _cache.Size;

        public GroupOfPicturesCache(IBufferPool? bufferPool)
        {
            _cache = new BufferCache<PictureCacheInfo>(bufferPool, 4096);
        }

        public void Add(PictureCacheInfo info, IDataBuffer buffer)
        {
            _cache.Write(info, buffer.UnderlyingBuffer.AsSpan(0, buffer.Size));
        }

        public void Clear()
        {
            _cache.Reset();
        }

        public IList<PictureCache> Get(int initialClaim = 1)
        {
            var pictureCaches = _cache.GetBuffers(initialClaim);
            return pictureCaches.Select(cache => new PictureCache(cache.Info.Type, cache.Info.Timestamp, cache.Buffer)).ToList();
        }

        public void Dispose()
        {
            _cache.Dispose();
        }
    }

    internal class RtmpStreamSubscriptionContext : IRtmpStreamSubscriptionContext
    {
        public uint StreamId { get; }
        public uint ChunkStreamId { get; }
        public string StreamPath { get; }
        public IReadOnlyDictionary<string, string> StreamArguments { get; }

        public bool IsPaused { get; set; }
        public bool IsReceivingAudio { get; set; }
        public bool IsReceivingVideo { get; set; }

        private readonly TaskCompletionSource _initializationTcs;
        private readonly Task _initializationTask;

        public RtmpStreamSubscriptionContext(uint streamId, uint chunkStreamId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            StreamId = streamId;
            ChunkStreamId = chunkStreamId;
            StreamPath = streamPath;
            StreamArguments = new Dictionary<string, string>(streamArguments);

            IsReceivingAudio = true;
            IsReceivingVideo = true;

            _initializationTcs = new TaskCompletionSource();
            _initializationTask = _initializationTcs.Task;
        }

        public void CompleteInitialization()
        {
            _initializationTcs.SetResult();
        }

        public Task UntilInitializationComplete()
        {
            return _initializationTask;
        }

        public ValueTask DisposeAsync()
        {
            _initializationTcs.TrySetCanceled();
            return ValueTask.CompletedTask;
        }
    }
}
