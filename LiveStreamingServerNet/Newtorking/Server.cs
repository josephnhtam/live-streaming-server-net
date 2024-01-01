using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Newtorking.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Newtorking
{
    public class Server : IServer
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

            using var tcpListener = CreateTcpListener(localEndpoint);
            tcpListener.Start();

            OnServerStarted();

            _logger?.LogInformation("Server is started");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await AcceptClientAsync(tcpListener, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger?.LogInformation("Server is shutting down");
            }
            catch (SocketException ex)
            {
                _logger?.LogError(ex, "An error occurred while accepting a client connection");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An error occurred in the server loop");
            }

            await Task.WhenAll(_clientPeerTasks.Select(x => x.Value.Task));

            _logger?.LogInformation("Server is stopped");
        }

        private async Task AcceptClientAsync(TcpListener tcpListener, CancellationToken cancellationToken)
        {
            var client = await tcpListener.AcceptTcpClientAsync(cancellationToken);
            OnClientAccepted(client);

            var clientPeerId = GetNextClientPeerId();
            var clientPeer = CreateClientPeer(clientPeerId, client);

            var clientPeerHandler = CreateClientPeerHandler(clientPeer);
            var clientTask = clientPeer.RunAsync(clientPeerHandler, cancellationToken);

            _clientPeerTasks.TryAdd(clientPeerId, new(clientPeer, clientTask));
            OnClientPeerConnected(clientPeer);

            _ = clientTask.ContinueWith(async _ =>
            {
                OnClientPeerDisconnected(clientPeer);
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

        private TcpListener CreateTcpListener(IPEndPoint localEndpoint)
        {
            var listener = new TcpListener(localEndpoint);
            OnListenerCreated(listener);
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

        protected virtual void OnListenerCreated(TcpListener tcpListener)
        {
            foreach (var handler in _serverEventHandlers)
            {
                handler.OnListenerCreated(tcpListener);
            }
        }

        protected virtual void OnClientAccepted(TcpClient tcpClient)
        {
            foreach (var handler in _serverEventHandlers)
            {
                handler.OnClientAccepted(tcpClient);
            }
        }

        protected virtual void OnClientPeerConnected(IClientPeer clientPeer)
        {
            foreach (var handler in _serverEventHandlers)
            {
                handler.OnClientPeerConnected(clientPeer);
            }
        }

        protected virtual void OnClientPeerDisconnected(IClientPeer clientPeer)
        {
            foreach (var handler in _serverEventHandlers)
            {
                handler.OnClientPeerDisconnected(clientPeer);
            }
        }

        protected virtual void OnServerStarted()
        {
            foreach (var handler in _serverEventHandlers)
            {
                handler.OnServerStarted();
            }
        }

        private record ClientPeerTask(IClientPeer Peer, Task Task);
    }
}
