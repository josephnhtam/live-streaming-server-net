using AutoFixture;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.Services
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

            var interceptor1 = Substitute.For<IRtmpMediaCachingInterceptor>();
            var interceptor2 = Substitute.For<IRtmpMediaCachingInterceptor>();

            var interceptors = new List<IRtmpMediaCachingInterceptor> { interceptor1, interceptor2 };

            var sut = new RtmpMediaCachingInterceptionService(interceptors);

            // Act
            await sut.CachePictureAsync(streamPath, mediaType, payloadBuffer, timestamp);

            // Assert
            await interceptor1.Received(1).OnCachePicture(streamPath, mediaType, Arg.Any<IRentedBuffer>(), timestamp);
            await interceptor2.Received(1).OnCachePicture(streamPath, mediaType, Arg.Any<IRentedBuffer>(), timestamp);
        }

        [Fact]
        public async Task CacheSequenceHeaderAsync_Should_InvokeOnCacheSequenceHeaderForEachInterceptor()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var mediaType = _fixture.Create<MediaType>();
            var sequenceHeader = _fixture.Create<byte[]>();

            var interceptor1 = Substitute.For<IRtmpMediaCachingInterceptor>();
            var interceptor2 = Substitute.For<IRtmpMediaCachingInterceptor>();

            var interceptors = new List<IRtmpMediaCachingInterceptor> { interceptor1, interceptor2 };

            var sut = new RtmpMediaCachingInterceptionService(interceptors);

            // Act
            await sut.CacheSequenceHeaderAsync(streamPath, mediaType, sequenceHeader);

            // Assert
            await interceptor1.Received(1).OnCacheSequenceHeader(streamPath, mediaType, sequenceHeader);
            await interceptor2.Received(1).OnCacheSequenceHeader(streamPath, mediaType, sequenceHeader);
        }

        [Fact]
        public async Task ClearGroupOfPicturesCacheAsync_Should_InvokeOnClearGroupOfPicturesCacheForEachInterceptor()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();

            var interceptor1 = Substitute.For<IRtmpMediaCachingInterceptor>();
            var interceptor2 = Substitute.For<IRtmpMediaCachingInterceptor>();

            var interceptors = new List<IRtmpMediaCachingInterceptor> { interceptor1, interceptor2 };

            var sut = new RtmpMediaCachingInterceptionService(interceptors);

            // Act
            await sut.ClearGroupOfPicturesCacheAsync(streamPath);

            // Assert
            await interceptor1.Received(1).OnClearGroupOfPicturesCache(streamPath);
            await interceptor2.Received(1).OnClearGroupOfPicturesCache(streamPath);
        }
    }
}
