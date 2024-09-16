using AutoFixture;
using FluentAssertions;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.ProtocolControls;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Server.Test.RtmpEventHandlers.ProtocolControls
{
    public class RtmpSetChunkSizeHandlerTest
    {
        [Fact]
        public async Task HandleAsync_Should_SetInChunkSizeAndLog()
        {
            // Arrange
            var logger = Substitute.For<ILogger<RtmpSetChunkSizeHandler>>();
            var chunkStreamContext = Substitute.For<IRtmpChunkStreamContext>();
            var clientContext = Substitute.For<IRtmpClientSessionContext>();
            var payloadBuffer = Substitute.For<IDataBuffer>();

            var fixture = new Fixture();
            var inChunkSize = fixture.Create<uint>();
            payloadBuffer.ReadUInt32BigEndian().Returns(inChunkSize);

            var sut = new RtmpSetChunkSizeHandler(logger);

            // Act
            var result = await sut.HandleAsync(chunkStreamContext, clientContext, payloadBuffer, default);

            // Assert
            result.Should().BeTrue();
            clientContext.Received(1).InChunkSize = inChunkSize;
        }
    }
}
