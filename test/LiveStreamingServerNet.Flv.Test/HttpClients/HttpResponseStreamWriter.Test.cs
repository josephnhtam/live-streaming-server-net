using AutoFixture;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.HttpClients;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace LiveStreamingServerNet.Flv.Test.HttpClients
{
    public class HttpResponseStreamWriterTest
    {
        private readonly IFixture _fixture;
        private readonly HttpResponse _response;
        private readonly IStreamWriter _sut;

        public HttpResponseStreamWriterTest()
        {
            _fixture = new Fixture();
            _response = Substitute.For<HttpResponse>();
            _sut = new HttpResponseStreamWriter(_response);
        }

        [Fact]
        public async Task WriteAsync_ShouldWriteToResponseBodyWriter()
        {
            // Arrange
            var buffer = _fixture.Create<ReadOnlyMemory<byte>>();

            // Act
            await _sut.WriteAsync(buffer, default);

            // Assert
            await _response.BodyWriter.Received(1).WriteAsync(buffer, Arg.Any<CancellationToken>());
        }
    }
}
