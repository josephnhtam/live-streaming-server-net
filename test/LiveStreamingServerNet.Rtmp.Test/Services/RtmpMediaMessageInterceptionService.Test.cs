using AutoFixture;
using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Utilities;
using LiveStreamingServerNet.Utilities.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.Services
{
    public class RtmpMediaMessageInterceptionServiceTest
    {
        private readonly IFixture _fixture;

        public RtmpMediaMessageInterceptionServiceTest()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public async Task CachePictureAsync_Should_InvokeOnCachePictureForEachInterceptor()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var mediaType = _fixture.Create<MediaType>();
            var rentedBuffer = new RentedBuffer(_fixture.Create<int>());
            var timestamp = _fixture.Create<uint>();

            var interceptor1 = Substitute.For<IRtmpMediaMessageInterceptor>();
            var interceptor2 = Substitute.For<IRtmpMediaMessageInterceptor>();

            var interceptors = new List<IRtmpMediaMessageInterceptor> { interceptor1, interceptor2 };

            var service = new RtmpMediaMessageInterceptionService(interceptors);

            // Act
            await service.CachePictureAsync(streamPath, mediaType, rentedBuffer, timestamp);

            // Assert
            await interceptor1.Received(1).OnCachePicture(streamPath, mediaType, rentedBuffer, timestamp);
            await interceptor2.Received(1).OnCachePicture(streamPath, mediaType, rentedBuffer, timestamp);
        }

        [Fact]
        public async Task CacheSequenceHeaderAsync_Should_InvokeOnCacheSequenceHeaderForEachInterceptor()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var mediaType = _fixture.Create<MediaType>();
            var sequenceHeader = _fixture.Create<byte[]>();

            var interceptor1 = Substitute.For<IRtmpMediaMessageInterceptor>();
            var interceptor2 = Substitute.For<IRtmpMediaMessageInterceptor>();

            var interceptors = new List<IRtmpMediaMessageInterceptor> { interceptor1, interceptor2 };

            var service = new RtmpMediaMessageInterceptionService(interceptors);

            // Act
            await service.CacheSequenceHeaderAsync(streamPath, mediaType, sequenceHeader);

            // Assert
            await interceptor1.Received(1).OnCacheSequenceHeader(streamPath, mediaType, sequenceHeader);
            await interceptor2.Received(1).OnCacheSequenceHeader(streamPath, mediaType, sequenceHeader);
        }

        [Fact]
        public async Task ClearGroupOfPicturesCacheAsync_Should_InvokeOnClearGroupOfPicturesCacheForEachInterceptor()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();

            var interceptor1 = Substitute.For<IRtmpMediaMessageInterceptor>();
            var interceptor2 = Substitute.For<IRtmpMediaMessageInterceptor>();

            var interceptors = new List<IRtmpMediaMessageInterceptor> { interceptor1, interceptor2 };

            var service = new RtmpMediaMessageInterceptionService(interceptors);

            // Act
            await service.ClearGroupOfPicturesCacheAsync(streamPath);

            // Assert
            await interceptor1.Received(1).OnClearGroupOfPicturesCache(streamPath);
            await interceptor2.Received(1).OnClearGroupOfPicturesCache(streamPath);
        }

        [Fact]
        public async Task ReceiveMediaMessageAsync_Should_InvokeOnReceiveMediaMessageForEachInterceptor()
        {
            // Arrange
            var streamPath = _fixture.Create<string>();
            var mediaType = _fixture.Create<MediaType>();
            using var payloadBuffer = new NetBuffer();
            var timestamp = _fixture.Create<uint>();
            var isSkippable = _fixture.Create<bool>();

            var interceptor1 = Substitute.For<IRtmpMediaMessageInterceptor>();
            var interceptor2 = Substitute.For<IRtmpMediaMessageInterceptor>();

            var interceptors = new List<IRtmpMediaMessageInterceptor> { interceptor1, interceptor2 };

            var service = new RtmpMediaMessageInterceptionService(interceptors);

            // Act
            await service.ReceiveMediaMessageAsync(streamPath, mediaType, payloadBuffer, timestamp, isSkippable);

            // Assert
            await interceptor1.Received(1).OnReceiveMediaMessage(streamPath, mediaType, Arg.Any<IRentedBuffer>(), timestamp, isSkippable);
            await interceptor2.Received(1).OnReceiveMediaMessage(streamPath, mediaType, Arg.Any<IRentedBuffer>(), timestamp, isSkippable);
        }
    }
}
