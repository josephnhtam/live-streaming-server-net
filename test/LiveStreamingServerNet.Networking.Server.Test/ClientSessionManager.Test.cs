using FluentAssertions;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using LiveStreamingServerNet.Networking.Server.Internal;
using LiveStreamingServerNet.Networking.Server.Internal.Contracts;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;

namespace LiveStreamingServerNet.Networking.Server.Test
{
    public class ClientSessionManagerTest
    {
        private readonly ServerEndPoint _serverEndPoint;
        private readonly ITcpListenerInternal _tcpListener;
        private readonly ITcpClientInternal _tcpClient;
        private readonly ISessionHandler _clientSessionHandler;
        private readonly ISessionHandlerFactory _clientSessionHandlerFactory;
        private readonly ISessionFactory _clientSessionFactory;
        private readonly ISession _clientSession;
        private readonly IServerEventDispatcher _eventDispatcher;
        private readonly ILogger<ClientSessionManager> _logger;
        private readonly IClientSessionManager _sut;

        public ClientSessionManagerTest()
        {
            _serverEndPoint = new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false);

            _tcpClient = Substitute.For<ITcpClientInternal>();
            _tcpListener = Substitute.For<ITcpListenerInternal>();
            _tcpListener.AcceptTcpClientAsync(Arg.Any<CancellationToken>()).Returns(_tcpClient);

            _clientSession = Substitute.For<ISession>();
            _clientSessionFactory = Substitute.For<ISessionFactory>();
            _clientSessionFactory.Create(Arg.Any<uint>(), _tcpClient, _serverEndPoint).Returns(_clientSession);

            _clientSessionHandler = Substitute.For<ISessionHandler>();
            _clientSessionHandlerFactory = Substitute.For<ISessionHandlerFactory>();
            _clientSessionHandlerFactory.Create(Arg.Is(_clientSession)).Returns(_clientSessionHandler);

            _eventDispatcher = Substitute.For<IServerEventDispatcher>();
            _logger = Substitute.For<ILogger<ClientSessionManager>>();

            _sut = new ClientSessionManager(_clientSessionFactory, _eventDispatcher, _logger);
        }

        [Fact]
        public async Task AcceptClientAsync_Should_RunClient_When_TcpClientIsAccepted()
        {
            // Act
            await _sut.AcceptClientAsync(_tcpListener, _serverEndPoint, default);

            // Assert
            _clientSessionFactory.Received(1).Create(Arg.Any<uint>(), _tcpClient, _serverEndPoint);
            _ = _clientSession.Received(1).RunAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task AcceptClientAsync_Should_DispatchServerEvents_When_TcpClientIsAccepted()
        {
            // Arrange
            var clientTcs = new TaskCompletionSource();
            _clientSession.RunAsync(Arg.Any<CancellationToken>()).Returns(clientTcs.Task);

            var tcs = new TaskCompletionSource();
            _eventDispatcher.When(x => x.ClientDisconnectedAsync(_clientSession)).Do(x => tcs.SetResult());

            // Act
            await _sut.AcceptClientAsync(_tcpListener, _serverEndPoint, default);

            // Assert
            Received.InOrder(() =>
            {
                _ = _eventDispatcher.Received(1).ClientAcceptedAsync(_tcpClient);
                _ = _eventDispatcher.Received(1).ClientConnectedAsync(_clientSession);
            });

            // Act
            clientTcs.SetCanceled();

            await _sut.WaitUntilAllClientTasksCompleteAsync(default);
            await tcs.Task;

            // Assert
            _ = _eventDispatcher.Received(1).ClientDisconnectedAsync(_clientSession);
        }

        [Fact]
        public async Task GetClient_Should_ReturnClientHandle_After_ClientAccepted()
        {
            // Arrange
            var clientTcs = new TaskCompletionSource();
            _clientSession.RunAsync(Arg.Any<CancellationToken>()).Returns(clientTcs.Task);

            uint? clientId = null;
            _clientSessionFactory.When(x => x.Create(Arg.Any<uint>(), _tcpClient, _serverEndPoint)).Do(callInfo => clientId = callInfo.ArgAt<uint>(0));

            // Act
            await _sut.AcceptClientAsync(_tcpListener, _serverEndPoint, default);

            // Assert
            _sut.GetClient(clientId!.Value).Should().Be(_clientSession);
        }

        [Fact]
        public async Task GetClientHandles_Should_ReturnClientHandles_After_ClientAccepted()
        {
            // Arrange
            var clientTcs = new TaskCompletionSource();
            _clientSession.RunAsync(Arg.Any<CancellationToken>()).Returns(clientTcs.Task);

            // Act
            await _sut.AcceptClientAsync(_tcpListener, _serverEndPoint, default);

            // Assert
            _sut.GetClients().Should().Contain(_clientSession);
        }
    }
}
