using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Internal;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;

namespace LiveStreamingServerNet.Networking.Test
{
    public class ClientTests : IDisposable
    {
        private readonly ServerEndPoint _serverEndPoint;
        private readonly IClientHandler _clientHandler;
        private readonly ITcpClientInternal _tcpClient;
        private readonly IClientBufferSender _bufferSender;
        private readonly INetworkStream _networkStream;
        private readonly INetworkStreamFactory _networkStreamFactory;
        private readonly ILogger<Client> _logger;
        private readonly IClient _sut;
        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _cancellationToken;

        public ClientTests()
        {
            _serverEndPoint = new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false);

            _clientHandler = Substitute.For<IClientHandler>();
            _tcpClient = Substitute.For<ITcpClientInternal>();
            _bufferSender = Substitute.For<IClientBufferSender>();
            _logger = Substitute.For<ILogger<Client>>();

            _tcpClient.Connected.Returns(true, false);
            _clientHandler.HandleClientLoopAsync(Arg.Any<INetworkStream>(), Arg.Any<CancellationToken>()).Returns(true, false);

            _networkStream = Substitute.For<INetworkStream>();
            _networkStreamFactory = Substitute.For<INetworkStreamFactory>();
            _networkStreamFactory
                .CreateNetworkStreamAsync(_tcpClient, _serverEndPoint, Arg.Any<CancellationToken>())
                .Returns(_networkStream);

            _sut = new Client(1, _tcpClient, _bufferSender, _networkStreamFactory, _logger);

            _cts = new CancellationTokenSource();
            _cancellationToken = _cts.Token;
        }

        [Fact]
        public async Task RunAsync_Should_InitializeClientHandler()
        {
            // Act
            await _sut.RunAsync(_clientHandler, _serverEndPoint, _cancellationToken);

            // Assert
            _ = _clientHandler.Received(1).InitializeAsync(_sut);
        }

        [Fact]
        public async Task RunAsync_Should_CreateNetworkStream()
        {
            // Act
            await _sut.RunAsync(_clientHandler, _serverEndPoint, _cancellationToken);

            // Assert
            _ = _networkStreamFactory.Received(1).CreateNetworkStreamAsync(_tcpClient, _serverEndPoint, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task RunAsync_Should_StartBufferSender()
        {
            // Arrange
            _networkStreamFactory.CreateNetworkStreamAsync(_tcpClient, _serverEndPoint, Arg.Any<CancellationToken>()).Returns(_networkStream);

            _tcpClient.Connected.Returns(false);

            // Act
            await _sut.RunAsync(_clientHandler, _serverEndPoint, _cancellationToken);

            // Assert
            _bufferSender.Received(1).Start(_networkStream, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task RunAsync_Should_RunClientLoop_Until_HandleClientLoopAsyncReturnsFalse()
        {
            // Arrange
            _tcpClient.Connected.Returns(true);

            _clientHandler.HandleClientLoopAsync(Arg.Any<INetworkStream>(), Arg.Any<CancellationToken>())
                .Returns(true, true, false);

            // Act
            await _sut.RunAsync(_clientHandler, _serverEndPoint, _cancellationToken);

            // Assert
            _ = _clientHandler.Received(3).HandleClientLoopAsync(Arg.Any<INetworkStream>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task RunAsync_Should_BeCancellable()
        {
            _tcpClient.Connected.Returns(true);
            _clientHandler.HandleClientLoopAsync(Arg.Any<INetworkStream>(), Arg.Any<CancellationToken>())
                .Returns(Task.Delay(100).ContinueWith(_ => true));

            // Act
            var clientTask = _sut.RunAsync(_clientHandler, _serverEndPoint, _cancellationToken);

            _cts.Cancel();
            await clientTask;

            // Assert
            _ = _clientHandler.Received().HandleClientLoopAsync(Arg.Any<INetworkStream>(), Arg.Any<CancellationToken>());

            _ = _bufferSender.Received().DisposeAsync();
            _ = _clientHandler.Received().DisposeAsync();
            _networkStream.Received().Dispose();
            _tcpClient.Received().Close();
        }

        [Fact]
        public async Task Disconnect_Should_CancelRunAsync()
        {
            _tcpClient.Connected.Returns(true);
            _clientHandler.HandleClientLoopAsync(Arg.Any<INetworkStream>(), Arg.Any<CancellationToken>())
                .Returns(Task.Delay(100).ContinueWith(_ => true));

            // Act
            var clientTask = _sut.RunAsync(_clientHandler, _serverEndPoint, _cancellationToken);

            _sut.Disconnect();
            await clientTask;

            // Assert
            _ = _bufferSender.Received().DisposeAsync();
            _ = _clientHandler.Received().DisposeAsync();
            _networkStream.Received().Dispose();
            _tcpClient.Received().Close();
        }

        public void Dispose()
        {
            _cts.Cancel();
        }
    }
}
