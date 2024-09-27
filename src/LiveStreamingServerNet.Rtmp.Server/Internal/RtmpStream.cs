using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal
{
    internal class RtmpStream : IRtmpStream
    {
        public uint Id { get; }

        public IRtmpClientSessionContext ClientContext { get; }
        public IRtmpPublishStreamContext? PublishContext { get; private set; }
        public IRtmpSubscribeStreamContext? SubscribeContext { get; private set; }

        private readonly IBufferPool? _bufferPool;
        private readonly Action<IRtmpStream>? _onDelete;

        public RtmpStream(uint streamId, IRtmpClientSessionContext clientContext, IBufferPool? bufferPool, Action<IRtmpStream>? onDelete)
        {
            Id = streamId;
            ClientContext = clientContext;
            _bufferPool = bufferPool;
            _onDelete = onDelete;
        }

        public IRtmpPublishStreamContext CreatePublishContext(string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            ValidateContextCreation();

            PublishContext = new RtmpPublishStreamContext(this, streamPath, streamArguments, _bufferPool);
            return PublishContext;
        }

        public IRtmpSubscribeStreamContext CreateSubscribeContext(string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            ValidateContextCreation();

            SubscribeContext = new RtmpSubscribeStreamContext(this, streamPath, streamArguments);
            return SubscribeContext;
        }

        public void RemovePublishContext()
        {
            if (PublishContext != null)
            {
                PublishContext.Dispose();
                PublishContext = null;
            }
        }

        public void RemoveSubscribeContext()
        {
            if (SubscribeContext != null)
            {
                SubscribeContext.Dispose();
                SubscribeContext = null;
            }
        }

        private void ValidateContextCreation()
        {
            if (PublishContext != null)
                throw new InvalidOperationException("Publish context does not exist.");

            if (SubscribeContext != null)
                throw new InvalidOperationException("Subscribe context does not exist.");
        }

        public void Delete()
        {
            _onDelete?.Invoke(this);

            RemovePublishContext();
            RemoveSubscribeContext();
        }
    }

    internal abstract class RtmpStreamContext : IRtmpStreamContext
    {
        public IRtmpStream Stream { get; }
        public string StreamPath { get; }
        public IReadOnlyDictionary<string, string> StreamArguments { get; }

        public uint VideoTimestamp => _videoTimestamp;
        public uint AudioTimestamp => _audioTimestamp;

        private uint _videoTimestamp;
        private uint _audioTimestamp;

        private readonly object _videoTimestampSyncLock = new();
        private readonly object _audioTimestampSyncLock = new();

        protected RtmpStreamContext(IRtmpStream stream, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            Stream = stream;
            StreamPath = streamPath;
            StreamArguments = streamArguments;
        }

        public bool UpdateTimestamp(uint timestamp, MediaType mediaType)
        {
            switch (mediaType)
            {
                case MediaType.Video:
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

        public virtual void Dispose() { }
    }

    internal class RtmpPublishStreamContext : RtmpStreamContext, IRtmpPublishStreamContext
    {
        public IReadOnlyDictionary<string, object>? StreamMetaData { get; set; }
        public byte[]? VideoSequenceHeader { get; set; }
        public byte[]? AudioSequenceHeader { get; set; }
        public bool GroupOfPicturesCacheActivated { get; set; }
        public IGroupOfPicturesCache GroupOfPicturesCache { get; }
        public DateTime StartTime { get; }

        public RtmpPublishStreamContext(IRtmpStream stream, string streamPath, IReadOnlyDictionary<string, string> streamArguments, IBufferPool? bufferPool) :
            base(stream, streamPath, streamArguments)
        {
            GroupOfPicturesCache = new GroupOfPicturesCache(bufferPool);
            StartTime = DateTime.UtcNow;
        }

        public override void Dispose()
        {
            base.Dispose();
            GroupOfPicturesCache.Dispose();
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

    internal class RtmpSubscribeStreamContext : RtmpStreamContext, IRtmpSubscribeStreamContext
    {
        public bool IsPaused { get; set; }
        public bool IsReceivingAudio { get; set; }
        public bool IsReceivingVideo { get; set; }

        private readonly TaskCompletionSource _initializationTcs;
        private readonly Task _initializationTask;

        public RtmpSubscribeStreamContext(IRtmpStream stream, string streamPath, IReadOnlyDictionary<string, string> streamArguments) :
            base(stream, streamPath, streamArguments)
        {
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

        public override void Dispose()
        {
            base.Dispose();
            _initializationTcs.TrySetCanceled();
        }
    }
}
