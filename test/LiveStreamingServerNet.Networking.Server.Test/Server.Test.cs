using FluentAssertions;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Server.Contracts;
using LiveStreamingServerNet.Networking.Server.Internal.Contracts;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;

namespace LiveStreamingServerNet.Networking.Server.Test
{
    using Server = Internal.Server;

    public class ServerTest
    {
        private readonly IServiceProvider _services;
        private readonly ITcpListenerFactory _tcpListenerFactory;
        private readonly IClientSessionManager _clientSessionManager;
        private readonly IServerEventDispatcher _eventDispatcher;
        private readonly ILogger<Server> _logger;
        private readonly IServer _sut;

        public ServerTest()
        {
            _services = Substitute.For<IServiceProvider>();
            _tcpListenerFactory = Substitute.For<ITcpListenerFactory>();
            _clientSessionManager = Substitute.For<IClientSessionManager>();
            _eventDispatcher = Substitute.For<IServerEventDispatcher>();
            _logger = Substitute.For<ILogger<Server>>();

            _sut = new Server(_services, _tcpListenerFactory, _clientSessionManager, _eventDispatcher, _logger);
        }

        [Theory]
        [MemberData(nameof(GetServerEndPoints))]
        public async Task RunAsync_Should_Create_TcpListeners(List<ServerEndPoint> serverEndPoints)
        {
            // Arrange
            _tcpListenerFactory.Create(Arg.Any<IPEndPoint>()).Returns(Substitute.For<ITcpListenerInternal>());

            // Act
            await _sut.RunAsync(serverEndPoints, new CancellationTokenSource(0).Token);

            // Assert
            foreach (var serverEndPoint in serverEndPoints)
            {
                _tcpListenerFactory.Received(1).Create(serverEndPoint.IPEndPoint);
            }
        }

        [Fact]
        public async Task RunAsync_Should_Dispatch_ServerEvents()
        {
            // Arrange
            var tcpListener = Substitute.For<ITcpListenerInternal>();
            _tcpListenerFactory.Create(Arg.Any<IPEndPoint>()).Returns(tcpListener);

            var serverEndPoint = new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false);

            // Act
            await _sut.RunAsync(serverEndPoint, new CancellationTokenSource(0).Token);

            // Assert
            Received.InOrder(() =>
            {
                _ = _eventDispatcher.Received(1).ListenerCreatedAsync(tcpListener);
                _ = _eventDispatcher.Received(1).ServerStartedAsync();
                _ = _eventDispatcher.Received(1).ServerStoppedAsync();
            });
        }

        [Fact]
        public async Task RunAsync_Should_AcceptClient()
        {
            // Arrange
            var serverEndPoint = new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false);
            var cts = new CancellationTokenSource();
            var cancellation = cts.Token;

            var tcpListener = Substitute.For<ITcpListenerInternal>();
            _tcpListenerFactory.Create(Arg.Any<IPEndPoint>()).Returns(tcpListener);

            _clientSessionManager.When(x => x.AcceptClientAsync(tcpListener, serverEndPoint, Arg.Any<CancellationToken>()))
                         .Do(_ => cts.Cancel());

            // Act
            await _sut.RunAsync(serverEndPoint, cancellation);

            // Assert
            _ = _clientSessionManager.Received().AcceptClientAsync(tcpListener, serverEndPoint, Arg.Any<CancellationToken>());
        }

        [Fact]
        public void GetClient_Should_ReturnClientHandle()
        {
            // Arrange
            uint clientId = 1;

            var expectedClientHandle = Substitute.For<ISessionHandle>();
            _clientSessionManager.GetClient(clientId).Returns(expectedClientHandle);

            // Act
            var result = _sut.GetClient(clientId);

            // Assert
            result.Should().Be(expectedClientHandle);
        }

        [Fact]
        public void Clients_Should_ReturnClientHandles()
        {
            // Arrange
            var expectedClientHandles = Substitute.For<IReadOnlyList<ISessionHandle>>();
            _clientSessionManager.GetClients().Returns(expectedClientHandles);

            // Act
            var result = _sut.Clients;

            // Assert
            result.Should().BeEquivalentTo(expectedClientHandles);
        }

        public static IEnumerable<object[]> GetServerEndPoints()
        {
            yield return new object[]{
                new List<ServerEndPoint> {
                    new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false),
                },
            };

            yield return new object[]{
                new List<ServerEndPoint> {
                    new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false),
                    new ServerEndPoint(new IPEndPoint(IPAddress.Any, 433), true),
                }
            };
        }
    }
}
