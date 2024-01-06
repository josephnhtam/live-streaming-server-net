using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Newtorking.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Newtorking
{
    public sealed class Server : IServer
    {
        private readonly ConcurrentDictionary<uint, ClientPeerTask> _clientPeerTasks = new();
        private readonly IServiceProvider _services;
        private readonly IClientPeerHandlerFactory _clientPeerHandlerFactory;
        private readonly IEnumerable<IServerEventHandler> _serverEventHandlers;
        private readonly ILogger _logger;
        private int _isStarted;
        private uint _nextClientPeerId;

        public bool IsStarted => _isStarted == 1;
        public IList<IClientPeerHandle> ClientPeers => _clientPeerTasks.Select(x => x.Value.Peer).OfType<IClientPeerHandle>().ToList();

        public Server(
            IServiceProvider services,
            IClientPeerHandlerFactory clientPeerHandlerFactory,
            IEnumerable<IServerEventHandler> serverEventHandlers,
            ILogger<Server> logger)
        {
            _services = services;
            _clientPeerHandlerFactory = clientPeerHandlerFactory;
            _serverEventHandlers = serverEventHandlers;
            _logger = logger;
        }

        public async Task RunAsync(IPEndPoint localEndpoint, CancellationToken cancellationToken = default)
        {
            ValidateAndSetStarted();

            using var tcpListener = await CreateTcpListener(localEndpoint);
            tcpListener.Start();

            await OnServerStartedAsync();

            _logger.ServerStarted(localEndpoint.ToString());

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await AcceptClientAsync(tcpListener, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.ServerShuttingDown();
            }
            catch (SocketException ex)
            {
                _logger.AcceptClientError(ex);
            }
            catch (Exception ex)
            {
                _logger.ServerLoopError(ex);
            }

            await Task.WhenAll(_clientPeerTasks.Select(x => x.Value.Task));

            await OnServerStoppedAsync();

            _logger.ServerStopped();
        }

        private async Task AcceptClientAsync(TcpListener tcpListener, CancellationToken cancellationToken)
        {
            var client = await tcpListener.AcceptTcpClientAsync(cancellationToken);
            await OnClientAcceptedAsync(client);

            var clientPeerId = GetNextClientPeerId();
            var clientPeer = CreateClientPeer(clientPeerId, client);

            var clientPeerHandler = CreateClientPeerHandler(clientPeer);
            var clientTask = clientPeer.RunAsync(clientPeerHandler, cancellationToken);

            _clientPeerTasks.TryAdd(clientPeerId, new(clientPeer, clientTask));
            await OnClientPeerConnectedAsync(clientPeer);

            _ = clientTask.ContinueWith(async _ =>
            {
                await OnClientPeerDisconnected(clientPeer);
                _clientPeerTasks.TryRemove(clientPeerId, out var removed);
                await clientPeer.DisposeAsync();
            });
        }

        private void ValidateAndSetStarted()
        {
            if (Interlocked.CompareExchange(ref _isStarted, 1, 0) == 1)
            {
                throw new InvalidOperationException("The server can only be started once.");
            }
        }

        private uint GetNextClientPeerId()
        {
            return Interlocked.Increment(ref _nextClientPeerId);
        }

        private async Task<TcpListener> CreateTcpListener(IPEndPoint localEndpoint)
        {
            var listener = new TcpListener(localEndpoint);
            await OnListenerCreatedAsync(listener);
            return listener;
        }

        private IClientPeer CreateClientPeer(uint clientPeerId, TcpClient tcpClient)
        {
            var clientPeer = _services.GetRequiredService<IClientPeer>();
            clientPeer.Initialize(clientPeerId, tcpClient);

            return clientPeer;
        }

        public IClientPeerHandle? GetClientPeer(uint clientPeerId)
        {
            return _clientPeerTasks.GetValueOrDefault(clientPeerId)?.Peer;
        }

        private IClientPeerHandler CreateClientPeerHandler(IClientPeerHandle clientPeer)
        {
            return _clientPeerHandlerFactory.CreateClientPeerHandler(clientPeer);
        }

        private async Task OnListenerCreatedAsync(TcpListener tcpListener)
        {
            foreach (var handler in _serverEventHandlers)
            {
                await handler.OnListenerCreatedAsync(tcpListener);
            }
        }

        private async Task OnClientAcceptedAsync(TcpClient tcpClient)
        {
            foreach (var handler in _serverEventHandlers)
            {
                await handler.OnClientAcceptedAsync(tcpClient);
            }
        }

        private async Task OnClientPeerConnectedAsync(IClientPeer clientPeer)
        {
            foreach (var handler in _serverEventHandlers)
            {
                await handler.OnClientPeerConnectedAsync(clientPeer);
            }
        }

        private async Task OnClientPeerDisconnected(IClientPeer clientPeer)
        {
            foreach (var handler in _serverEventHandlers)
            {
                await handler.OnClientPeerDisconnectedAsync(clientPeer);
            }
        }

        private async Task OnServerStartedAsync()
        {
            foreach (var handler in _serverEventHandlers)
            {
                await handler.OnServerStartedAsync();
            }
        }

        private async Task OnServerStoppedAsync()
        {
            foreach (var handler in _serverEventHandlers)
            {
                await handler.OnServerStoppedAsync();
            }
        }

        private record ClientPeerTask(IClientPeer Peer, Task Task);
    }
}
