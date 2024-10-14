﻿using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using System.Runtime.CompilerServices;

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

        private bool _isReady;
        private TaskCompletionSource _readyTcs = new();

        public FlvStreamContext(string streamPath, IReadOnlyDictionary<string, string> streamArguments, IBufferPool? bufferPool)
        {
            StreamPath = streamPath;
            StreamArguments = new Dictionary<string, string>(streamArguments);
            GroupOfPicturesCache = new GroupOfPicturesCache(bufferPool);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetReady()
        {
            if (_isReady)
                return;

            _readyTcs.TrySetResult();
            _isReady = true;
        }

        public Task UntilReadyAsync()
        {
            return _readyTcs.Task;
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
