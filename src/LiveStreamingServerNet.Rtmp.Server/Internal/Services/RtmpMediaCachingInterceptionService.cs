﻿using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpMediaCachingInterceptionService : IRtmpMediaCachingInterceptionService
    {
        private readonly IEnumerable<IRtmpMediaCachingInterceptor> _interceptors;

        public RtmpMediaCachingInterceptionService(IEnumerable<IRtmpMediaCachingInterceptor> interceptors)
        {
            _interceptors = interceptors;
        }

        public async ValueTask CachePictureAsync(string streamPath, MediaType mediaType, IDataBuffer payloadBuffer, uint timestamp)
        {
            if (!_interceptors.Any())
                return;

            var rentedBuffer = payloadBuffer.ToRentedBuffer();

            try
            {
                foreach (var interceptor in _interceptors)
                    await interceptor.OnCachePictureAsync(streamPath, mediaType, rentedBuffer, timestamp);
            }
            finally
            {
                rentedBuffer.Unclaim();
            }
        }

        public async ValueTask CacheSequenceHeaderAsync(string streamPath, MediaType mediaType, byte[] sequenceHeader)
        {
            foreach (var interceptor in _interceptors)
                await interceptor.OnCacheSequenceHeaderAsync(streamPath, mediaType, sequenceHeader);
        }

        public async ValueTask ClearGroupOfPicturesCacheAsync(string streamPath)
        {
            foreach (var interceptor in _interceptors)
                await interceptor.OnClearGroupOfPicturesCacheAsync(streamPath);
        }
    }
}
