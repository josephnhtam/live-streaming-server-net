using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.ProtocolControls;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.RtmpEventHandlers.ProtocolControls
{
    public class RtmpWindowAcknowledgementSizeHandlerTest
    {
        [Fact]
        public async Task HandleAsync_Should_SetInWindowAcknowledgementSize()
        {
            // Arrange
            var fixture = new Fixture();
            var windowAcknowledgementSize = fixture.Create<uint>();

            var logger = Substitute.For<ILogger<RtmpWindowAcknowledgementSizeHandler>>();
            var chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            var clientContext = Substitute.For<IRtmpClientContext>();

            var payloadBuffer = Substitute.For<INetBuffer>();
            payloadBuffer.ReadUInt32BigEndian().Returns(windowAcknowledgementSize);

            var sut = new RtmpWindowAcknowledgementSizeHandler(logger);

            // Act
            var result = await sut.HandleAsync(chunkStreamContext, clientContext, payloadBuffer, default);

            // Assert
            result.Should().BeTrue();
            clientContext.Received(1).InWindowAcknowledgementSize = windowAcknowledgementSize;
        }
    }
}
