using AutoFixture;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.WebSocketClients;
using NSubstitute;
using System.Net.WebSockets;

namespace LiveStreamingServerNet.Flv.Test.WebSocketClients
{
    public class WebSocketStreamWriterTest
    {
        private readonly IFixture _fixture;
        private readonly WebSocket _webSocket;
        private readonly IStreamWriter _sut;

        public WebSocketStreamWriterTest()
        {
            _fixture = new Fixture();
            _webSocket = Substitute.For<WebSocket>();
            _sut = new WebSocketStreamWriter(_webSocket);
        }

        [Fact]
        public async Task WriteAsync_ShouldWriteToResponseBodyWriter()
        {
            // Arrange
            var buffer = _fixture.Create<ReadOnlyMemory<byte>>();

            // Act
            await _sut.WriteAsync(buffer, default);

            // Assert
            await _webSocket.Received(1).SendAsync(buffer, WebSocketMessageType.Binary, true, Arg.Any<CancellationToken>());
        }
    }
}
