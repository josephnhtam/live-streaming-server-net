﻿using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Extensions;

namespace LiveStreamingServerNet.Flv.Internal
{
    internal class FlvStreamContext : IFlvStreamContext
    {
        public string StreamPath { get; }
        public IReadOnlyDictionary<string, string> StreamArguments { get; }
        public IReadOnlyDictionary<string, object>? StreamMetaData { get; set; }
        public byte[]? VideoSequenceHeader { get; set; }
        public byte[]? AudioSequenceHeader { get; set; }
        public IGroupOfPicturesCache GroupOfPicturesCache { get; }
        public bool IsReady => _isReady || _readyTcs.Task.IsCompletedSuccessfully;
        public uint VideoTimestamp => _videoTimestamp;
        public uint AudioTimestamp => _audioTimestamp;
        public uint TimestampOffset => _timestampOffset;

        private uint _videoTimestamp;
        private uint _audioTimestamp;
        private uint _timestampOffset;

        private bool _isReady;
        private TaskCompletionSource _readyTcs = new();

        public FlvStreamContext(string streamPath, IReadOnlyDictionary<string, string> streamArguments, IBufferPool? bufferPool)
        {
            StreamPath = streamPath;
            StreamArguments = new Dictionary<string, string>(streamArguments);
            GroupOfPicturesCache = new GroupOfPicturesCache(bufferPool);
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

                if (newTimestamp < original)
                {
                    return false;
                }

                if (Interlocked.CompareExchange(ref currentTimestamp, newTimestamp, original) == original)
                {
                    return true;
                }
            }
        }

        public void SetTimestampOffset(uint timestampOffset)
        {
            _timestampOffset = timestampOffset;
        }

        public void SetReady()
        {
            if (_isReady)
                return;

            _readyTcs.TrySetResult();
            _isReady = true;
        }

        public Task UntilReadyAsync(CancellationToken cancellationToken)
        {
            return _readyTcs.Task.WithCancellation(cancellationToken);
        }

        public void Dispose()
        {
            _readyTcs.TrySetCanceled();
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

        public void Add(PictureCacheInfo info, byte[] buffer, int start, int length)
        {
            _cache.Write(info, buffer.AsSpan(start, length));
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
}
