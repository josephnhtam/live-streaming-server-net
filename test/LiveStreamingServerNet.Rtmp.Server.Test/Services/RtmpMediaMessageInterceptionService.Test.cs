﻿using AutoFixture;
using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services;
using LiveStreamingServerNet.Utilities.Buffers;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Server.Test.Services
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

            var publishStreamContext = Substitute.For<IRtmpPublishStreamContext>();
            publishStreamContext.StreamPath.Returns(streamPath);

            var interceptor1 = Substitute.For<IRtmpMediaMessageInterceptor>();
            var interceptor2 = Substitute.For<IRtmpMediaMessageInterceptor>();

            interceptor1.FilterMediaMessage(streamPath, mediaType, timestamp, isSkippable).Returns(true);
            interceptor2.FilterMediaMessage(streamPath, mediaType, timestamp, isSkippable).Returns(true);

            var interceptors = new List<IRtmpMediaMessageInterceptor> { interceptor1, interceptor2 };

            var service = new RtmpMediaMessageInterceptionService(interceptors);

            // Act
            await service.ReceiveMediaMessageAsync(publishStreamContext, mediaType, payloadBuffer, timestamp, isSkippable);

            // Assert
            await interceptor1.Received(1).OnReceiveMediaMessageAsync(streamPath, mediaType, Arg.Any<IRentedBuffer>(), timestamp, isSkippable);
            await interceptor2.Received(1).OnReceiveMediaMessageAsync(streamPath, mediaType, Arg.Any<IRentedBuffer>(), timestamp, isSkippable);
        }
    }
}
