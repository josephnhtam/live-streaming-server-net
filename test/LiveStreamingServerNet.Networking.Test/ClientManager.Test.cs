using FluentAssertions;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Internal;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;

namespace LiveStreamingServerNet.Networking.Test
{
    public class ClientManagerTest
    {
        private readonly ServerEndPoint _serverEndPoint;
        private readonly ITcpListenerInternal _tcpListener;
        private readonly ITcpClientInternal _tcpClient;
        private readonly IClientHandler _clientHandler;
        private readonly IClientHandlerFactory _clientHandlerFactory;
        private readonly IClientFactory _clientFactory;
        private readonly IClient _client;
        private readonly IServerEventDispatcher _eventDispatcher;
        private readonly ILogger<ClientManager> _logger;
        private readonly IClientManager _sut;

        public ClientManagerTest()
        {
            _serverEndPoint = new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false);

            _tcpClient = Substitute.For<ITcpClientInternal>();
            _tcpListener = Substitute.For<ITcpListenerInternal>();
            _tcpListener.AcceptTcpClientAsync(Arg.Any<CancellationToken>()).Returns(_tcpClient);

            _client = Substitute.For<IClient>();
            _clientFactory = Substitute.For<IClientFactory>();
            _clientFactory.Create(Arg.Any<uint>(), _tcpClient).Returns(_client);

            _clientHandler = Substitute.For<IClientHandler>();
            _clientHandlerFactory = Substitute.For<IClientHandlerFactory>();
            _clientHandlerFactory.CreateClientHandler().Returns(_clientHandler);

            _eventDispatcher = Substitute.For<IServerEventDispatcher>();
            _logger = Substitute.For<ILogger<ClientManager>>();

            _sut = new ClientManager(_clientFactory, _clientHandlerFactory, _eventDispatcher, _logger);
        }

        [Fact]
        public async Task AcceptClientAsync_Should_RunClient_When_TcpClientIsAccepted()
        {
            // Act
            await _sut.AcceptClientAsync(_tcpListener, _serverEndPoint, default);

            // Assert
            _clientFactory.Received(1).Create(Arg.Any<uint>(), _tcpClient);
            _ = _client.Received(1).RunAsync(_clientHandler, _serverEndPoint, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task AcceptClientAsync_Should_DispatchServerEvents_When_TcpClientIsAccepted()
        {
            // Arrange
            var clientTcs = new TaskCompletionSource();
            _client.RunAsync(_clientHandler, _serverEndPoint, Arg.Any<CancellationToken>()).Returns(clientTcs.Task);

            var tcs = new TaskCompletionSource();
            _eventDispatcher.When(x => x.ClientDisconnectedAsync(_client)).Do(x => tcs.SetResult());

            // Act
            await _sut.AcceptClientAsync(_tcpListener, _serverEndPoint, default);

            // Assert
            Received.InOrder(() =>
            {
                _ = _eventDispatcher.Received(1).ClientAcceptedAsync(_tcpClient);
                _ = _eventDispatcher.Received(1).ClientConnectedAsync(_client);
            });

            // Act
            clientTcs.SetCanceled();

            await _sut.WaitUntilAllClientTasksCompleteAsync(default);
            await tcs.Task;

            // Assert
            _ = _eventDispatcher.Received(1).ClientDisconnectedAsync(_client);
        }

        [Fact]
        public async Task GetClient_Should_ReturnClientHandle_After_ClientAccepted()
        {
            // Arrange
            var clientTcs = new TaskCompletionSource();
            _client.RunAsync(_clientHandler, _serverEndPoint, Arg.Any<CancellationToken>()).Returns(clientTcs.Task);

            uint? clientId = null;
            _clientFactory.When(x => x.Create(Arg.Any<uint>(), _tcpClient)).Do(callInfo => clientId = callInfo.ArgAt<uint>(0));

            // Act
            await _sut.AcceptClientAsync(_tcpListener, _serverEndPoint, default);

            // Assert
            _sut.GetClient(clientId!.Value).Should().Be(_client);
        }

        [Fact]
        public async Task GetClientHandles_Should_ReturnClientHandles_After_ClientAccepted()
        {
            // Arrange
            var clientTcs = new TaskCompletionSource();
            _client.RunAsync(_clientHandler, _serverEndPoint, Arg.Any<CancellationToken>()).Returns(clientTcs.Task);

            // Act
            await _sut.AcceptClientAsync(_tcpListener, _serverEndPoint, default);

            // Assert
            _sut.GetClientHandles().Should().Contain(_client);
        }
    }
}
