using LiveStreamingServerNet.Networking.Installer.Contracts;
using LiveStreamingServerNet.Networking.Internal;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using NSubstitute;
using System.Net;
using System.Net.Security;

namespace LiveStreamingServerNet.Networking.Test
{
    public class NetworkStreamFactoryTest : IDisposable
    {
        private readonly SslStream _sslStream;
        private ITcpClientInternal _tcpClient;
        private ISslStreamFactory _sslStreamFactory;

        public NetworkStreamFactoryTest()
        {
            _sslStream = new SslStream(new MemoryStream());

            _tcpClient = Substitute.For<ITcpClientInternal>();

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
