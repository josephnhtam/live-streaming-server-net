﻿using AutoFixture;
using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Server.Test.Services
{
    public class RtmpMediaCachingInterceptionServiceTest
    {
        private readonly IFixture _fixture;

        public RtmpMediaCachingInterceptionServiceTest()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public async Task CachePictureAsync_Should_InvokeOnCachePictureForEachInterceptor()
        {
            // Arrange
            using var payloadBuffer = new DataBuffer();
            var streamPath = _fixture.Create<string>();
            var mediaType = _fixture.Create<MediaType>();
            var timestamp = _fixture.Create<uint>();

            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            publishStreamContext.StreamPath.Returns(streamPath);

            var interceptor1 = Substitute.For<IRtmpMediaCachingInterceptor>();
            var interceptor2 = Substitute.For<IRtmpMediaCachingInterceptor>();

            interceptor1.FilterCache(streamPath, mediaType).Returns(true);
            interceptor2.FilterCache(streamPath, mediaType).Returns(true);

            var interceptors = new List<IRtmpMediaCachingInterceptor> { interceptor1, interceptor2 };

            var sut = new RtmpMediaCachingInterceptionService(interceptors);

            // Act
            await sut.CachePictureAsync(publishStreamContext, mediaType, payloadBuffer, timestamp);

            // Assert
            await interceptor1.Received(1).OnCachePictureAsync(streamPath, mediaType, Arg.Any<IRentedBuffer>(), timestamp);
            await interceptor2.Received(1).OnCachePictureAsync(streamPath, mediaType, Arg.Any<IRentedBuffer>(), timestamp);
        }

        [Fact]
        public async Task CacheSequenceHeaderAsync_Should_InvokeOnCacheSequenceHeaderForEachInterceptor()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var mediaType = _fixture.Create<MediaType>();
            var sequenceHeader = _fixture.Create<byte[]>();

            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            publishStreamContext.StreamPath.Returns(streamPath);

            var interceptor1 = Substitute.For<IRtmpMediaCachingInterceptor>();
            var interceptor2 = Substitute.For<IRtmpMediaCachingInterceptor>();

            interceptor1.FilterCache(streamPath, mediaType).Returns(true);
            interceptor2.FilterCache(streamPath, mediaType).Returns(true);

            var interceptors = new List<IRtmpMediaCachingInterceptor> { interceptor1, interceptor2 };

            var sut = new RtmpMediaCachingInterceptionService(interceptors);

            // Act
            await sut.CacheSequenceHeaderAsync(publishStreamContext, mediaType, sequenceHeader);

            // Assert
            await interceptor1.Received(1).OnCacheSequenceHeaderAsync(streamPath, mediaType, sequenceHeader);
            await interceptor2.Received(1).OnCacheSequenceHeaderAsync(streamPath, mediaType, sequenceHeader);
        }

        [Fact]
        public async Task ClearGroupOfPicturesCacheAsync_Should_InvokeOnClearGroupOfPicturesCacheForEachInterceptor()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();

            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            publishStreamContext.StreamPath.Returns(streamPath);

            var interceptor1 = Substitute.For<IRtmpMediaCachingInterceptor>();
            var interceptor2 = Substitute.For<IRtmpMediaCachingInterceptor>();

            var interceptors = new List<IRtmpMediaCachingInterceptor> { interceptor1, interceptor2 };

            var sut = new RtmpMediaCachingInterceptionService(interceptors);

            // Act
            await sut.ClearGroupOfPicturesCacheAsync(publishStreamContext);

            // Assert
            await interceptor1.Received(1).OnClearGroupOfPicturesCacheAsync(streamPath);
            await interceptor2.Received(1).OnClearGroupOfPicturesCacheAsync(streamPath);
        }
    }
}
