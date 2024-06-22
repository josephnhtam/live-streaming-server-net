using AutoFixture;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
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
        public async Task ReceiveMediaMessageAsync_Should_InvokeOnReceiveMediaMessageForEachInterceptor()
        {
            // Arrange
            using var payloadBuffer = new DataBuffer();
            var streamPath = _fixture.Create<string>();
            var mediaType = _fixture.Create<MediaType>();
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
