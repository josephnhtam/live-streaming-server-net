using LiveStreamingServerNet.Networking.Internal;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using NSubstitute;
using System.Net;
using System.Net.Security;

namespace LiveStreamingServerNet.Networking.Test
{
    public class NetworkStreamFactoryTest : IDisposable
    {
        private readonly MemoryStream _stream;
        private readonly SslStream _sslStream;
        private readonly ITcpClientInternal _tcpClient;
        private readonly ISslStreamFactory _sslStreamFactory;

        public NetworkStreamFactoryTest()
        {
            _stream = new MemoryStream();
            _sslStream = Substitute.For<SslStream>(_stream);
            _sslStream.CanRead.Returns(true);
            _sslStream.CanWrite.Returns(true);

            _tcpClient = Substitute.For<ITcpClientInternal>();
            _tcpClient.GetStream().Returns(_stream);

            _sslStreamFactory = Substitute.For<ISslStreamFactory>();
            _sslStreamFactory.CreateAsync(_tcpClient, Arg.Any<CancellationToken>()).Returns(_sslStream);
        }

        [Fact]
        public async Task CreateNetworkStreamAsync_WhenIsSecure_ReturnsSslStream()
        {
            // Arrange
            var serverEndPoint = new ServerEndPoint(new IPEndPoint(IPAddress.Any, 433), true);
            var sut = new NetworkStreamFactory(_sslStreamFactory);

            // Act
            var result = await sut.CreateNetworkStreamAsync(_tcpClient, serverEndPoint, default);

            // Assert
            _ = _sslStreamFactory.Received(1).CreateAsync(_tcpClient, Arg.Any<CancellationToken>());
            Assert.Equal(_sslStream, result.InnerStream);
        }

        [Fact]
        public async Task CreateNetworkStreamAsync_WhenIsNotSecure_ReturnsNonSslStream()
        {
            // Arrange
            var serverEndPoint = new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false);
            var sut = new NetworkStreamFactory(_sslStreamFactory);

            // Act
            await sut.CreateNetworkStreamAsync(_tcpClient, serverEndPoint, default);

            // Assert
            _ = _sslStreamFactory.DidNotReceive().CreateAsync(_tcpClient, Arg.Any<CancellationToken>());
        }

        public void Dispose()
        {
            _sslStream.Dispose();
        }
    }
}
