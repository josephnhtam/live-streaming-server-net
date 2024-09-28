﻿using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal
{
    internal class RtmpStreamContext : IRtmpStreamContext
    {
        public uint StreamId { get; }

        public IRtmpClientSessionContext ClientContext { get; }
        public IRtmpPublishStreamContext? PublishContext { get; private set; }
        public IRtmpSubscribeStreamContext? SubscribeContext { get; private set; }

        private readonly IBufferPool? _bufferPool;

        public RtmpStreamContext(uint streamId, IRtmpClientSessionContext clientContext, IBufferPool? bufferPool)
        {
            StreamId = streamId;
            ClientContext = clientContext;
            _bufferPool = bufferPool;
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
                throw new InvalidOperationException("Publish context already exists.");

            if (SubscribeContext != null)
                throw new InvalidOperationException("Subscribe context already exists.");
        }

        public void Dispose()
        {
            RemovePublishContext();
            RemoveSubscribeContext();
        }
    }

    internal abstract class RtmpMediaStreamContext : IRtmpMediaStreamContext
    {
        public IRtmpStreamContext StreamContext { get; }
        public string StreamPath { get; }
        public IReadOnlyDictionary<string, string> StreamArguments { get; }

        public uint VideoTimestamp => _videoTimestamp;
        public uint AudioTimestamp => _audioTimestamp;

        private uint _videoTimestamp;
        private uint _audioTimestamp;

        protected RtmpMediaStreamContext(IRtmpStreamContext streamContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            StreamContext = streamContext;
            StreamPath = streamPath;
            StreamArguments = streamArguments;
        }

        public bool UpdateTimestamp(uint timestamp, MediaType mediaType)
        {
            switch (mediaType)
            {
                case MediaType.Video:
                    return UpdateTimestamp(ref _videoTimestamp, timestamp);

                case MediaType.Audio:
                    return UpdateTimestamp(ref _audioTimestamp, timestamp);

                default:
                    throw new ArgumentOutOfRangeException(nameof(mediaType), mediaType, null);
            }
        }

        private static bool UpdateTimestamp(ref uint currentTimestamp, uint newTimestamp)
        {
            while (true)
            {
                var original = currentTimestamp;

                if (newTimestamp <= original)
                {
                    return false;
                }

                if (Interlocked.CompareExchange(ref currentTimestamp, newTimestamp, original) == original)
                {
                    return true;
                }
            }
        }

        public virtual void Dispose() { }
    }

    internal class RtmpPublishStreamContext : RtmpMediaStreamContext, IRtmpPublishStreamContext
    {
        public IReadOnlyDictionary<string, object>? StreamMetaData { get; set; }
        public byte[]? VideoSequenceHeader { get; set; }
        public byte[]? AudioSequenceHeader { get; set; }
        public bool GroupOfPicturesCacheActivated { get; set; }
        public IGroupOfPicturesCache GroupOfPicturesCache { get; }
        public DateTime StartTime { get; }

        public RtmpPublishStreamContext(IRtmpStreamContext streamContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments, IBufferPool? bufferPool) :
            base(streamContext, streamPath, streamArguments)
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

    internal class RtmpSubscribeStreamContext : RtmpMediaStreamContext, IRtmpSubscribeStreamContext
    {
        public bool IsPaused { get; set; }
        public bool IsReceivingAudio { get; set; }
        public bool IsReceivingVideo { get; set; }

        private readonly TaskCompletionSource _initializationTcs;
        private readonly Task _initializationTask;

        public RtmpSubscribeStreamContext(IRtmpStreamContext streamContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments) :
            base(streamContext, streamPath, streamArguments)
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
