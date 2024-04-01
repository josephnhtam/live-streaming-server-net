using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Handshakes;
using System.Collections.Concurrent;

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

        public IRtmpPublishStreamContext? PublishStreamContext { get; private set; }
        public IRtmpStreamSubscriptionContext? StreamSubscriptionContext { get; private set; }

        private uint _streamId;
        private bool _isStreamCreated;
        private readonly ConcurrentDictionary<uint, IRtmpChunkStreamContext> _chunkStreamContexts = new();

        public RtmpClientContext(IClientHandle client)
        {
            Client = client;
            State = RtmpClientState.HandshakeC0;
        }

        public IRtmpPublishStreamContext CreatePublishStreamContext(string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var newContext = new RtmpPublishStreamContext(_streamId, streamPath, streamArguments);
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
            return _chunkStreamContexts.GetOrAdd(chunkStreamId, new RtmpChunkStreamContext(chunkStreamId));
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

        public RtmpPublishStreamContext(uint streamId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            StreamId = streamId;
            StreamPath = streamPath;
            StreamArguments = streamArguments;
            GroupOfPicturesCache = new GroupOfPicturesCache();
        }
    }

    internal class GroupOfPicturesCache : IGroupOfPicturesCache
    {
        public long Size { get; private set; }
        private readonly Queue<PicturesCache> _groupOfPicturesCache = new();

        public void Add(PicturesCache cache)
        {
            lock (_groupOfPicturesCache)
            {
                _groupOfPicturesCache.Enqueue(cache);
                Size += cache.Payload.Size;
            }
        }

        public void Clear(bool unclaim)
        {
            lock (_groupOfPicturesCache)
            {
                if (unclaim)
                {
                    foreach (var cache in _groupOfPicturesCache)
                        cache.Payload.Unclaim();
                }

                _groupOfPicturesCache.Clear();
                Size = 0;
            }
        }

        public IList<PicturesCache> Get(bool claim)
        {
            lock (_groupOfPicturesCache)
            {
                if (claim)
                {
                    foreach (var cache in _groupOfPicturesCache)
                        cache.Payload.Claim();
                }

                return new List<PicturesCache>(_groupOfPicturesCache);
            }
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
            StreamArguments = streamArguments;

            IsPaused = false;
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
    }
}
