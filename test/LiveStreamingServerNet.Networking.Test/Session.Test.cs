using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Internal;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;

namespace LiveStreamingServerNet.Networking.Test
{
    public class ClientSessionTests : IDisposable
    {
        private readonly ServerEndPoint _serverEndPoint;
        private readonly ISessionHandler _clientSessionHandler;
        private readonly ISessionHandlerFactory _clientSessionHandlerFactory;
        private readonly ITcpClientInternal _tcpClient;
        private readonly IBufferSender _bufferSender;
        private readonly IBufferSenderFactory _bufferSenderFactory;
        private readonly INetworkStream _networkStream;
        private readonly INetworkStreamFactory _networkStreamFactory;
        private readonly ILogger<Session> _logger;
        private readonly ISession _sut;
        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _cancellationToken;

        public ClientSessionTests()
        {
            _serverEndPoint = new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false);

            _clientSessionHandler = Substitute.For<ISessionHandler>();
            _clientSessionHandlerFactory = Substitute.For<ISessionHandlerFactory>();
            _tcpClient = Substitute.For<ITcpClientInternal>();
            _bufferSender = Substitute.For<IBufferSender>();
            _logger = Substitute.For<ILogger<Session>>();

            _tcpClient.Connected.Returns(true, false);
            _clientSessionHandler.HandleSessionLoopAsync(Arg.Any<INetworkStream>(), Arg.Any<CancellationToken>()).Returns(true, false);

            _bufferSenderFactory = Substitute.For<IBufferSenderFactory>();
            _bufferSenderFactory.Create().Returns(_bufferSender);

            _networkStream = Substitute.For<INetworkStream>();
            _networkStreamFactory = Substitute.For<INetworkStreamFactory>();
            _networkStreamFactory
                .CreateNetworkStreamAsync(1, _tcpClient, _serverEndPoint, Arg.Any<CancellationToken>())
                .Returns(_networkStream);

            _sut = new Session(1, _tcpClient, _serverEndPoint, _bufferSenderFactory, _networkStreamFactory, _clientSessionHandlerFactory, _logger);
            _clientSessionHandlerFactory.Create(Arg.Is(_sut)).Returns(_clientSessionHandler);

            _cts = new CancellationTokenSource();
            _cancellationToken = _cts.Token;
        }

        [Fact]
        public async Task RunAsync_Should_InitializeClientHandler()
        {
            // Act
            await _sut.RunAsync(_cancellationToken);

            // Assert
            _ = _clientSessionHandler.Received(1).InitializeAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task RunAsync_Should_CreateNetworkStream()
        {
            // Act
            await _sut.RunAsync(_cancellationToken);

            // Assert
            _ = _networkStreamFactory.Received(1).CreateNetworkStreamAsync(1, _tcpClient, _serverEndPoint, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task RunAsync_Should_StartBufferSender()
        {
            // Arrange
            _networkStreamFactory.CreateNetworkStreamAsync(1, _tcpClient, _serverEndPoint, Arg.Any<CancellationToken>()).Returns(_networkStream);

            _tcpClient.Connected.Returns(false);

            // Act
            await _sut.RunAsync(_cancellationToken);

            // Assert
            _bufferSender.Received(1).Start(_networkStream, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task RunAsync_Should_RunClientLoop_Until_HandleClientLoopAsyncReturnsFalse()
        {
            // Arrange
            _tcpClient.Connected.Returns(true);

            _clientSessionHandler.HandleSessionLoopAsync(Arg.Any<INetworkStream>(), Arg.Any<CancellationToken>())
                .Returns(true, true, false);

            // Act
            await _sut.RunAsync(_cancellationToken);

            // Assert
            _ = _clientSessionHandler.Received(3).HandleSessionLoopAsync(Arg.Any<INetworkStream>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task RunAsync_Should_BeCancellable()
        {
            _tcpClient.Connected.Returns(true);
            _clientSessionHandler.HandleSessionLoopAsync(Arg.Any<INetworkStream>(), Arg.Any<CancellationToken>())
                .Returns(Task.Delay(100).ContinueWith(_ => true));

            // Act
            var clientTask = _sut.RunAsync(_cancellationToken);

            _cts.Cancel();
            await clientTask;

            // Assert
            _ = _clientSessionHandler.Received().HandleSessionLoopAsync(Arg.Any<INetworkStream>(), Arg.Any<CancellationToken>());

            _ = _clientSessionHandler.Received().DisposeAsync();
            _ = _bufferSender.Received().DisposeAsync();
            _ = _networkStream.Received().DisposeAsync();
            _tcpClient.Received().Close();
        }

        [Fact]
        public async Task Disconnect_Should_CancelRunAsync()
        {
            _tcpClient.Connected.Returns(true);
            _clientSessionHandler.HandleSessionLoopAsync(Arg.Any<INetworkStream>(), Arg.Any<CancellationToken>())
                .Returns(Task.Delay(100).ContinueWith(_ => true));

            // Act
            var clientTask = _sut.RunAsync(_cancellationToken);

            _sut.Disconnect();
            await clientTask;

            // Assert
            _ = _clientSessionHandler.Received().DisposeAsync();
            _ = _bufferSender.Received().DisposeAsync();
            _ = _networkStream.Received().DisposeAsync();
            _tcpClient.Received().Close();
        }

        public void Dispose()
        {
            _cts.Cancel();
        }
    }
}
