using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Utilities;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServer.Newtorking
{
    public abstract class Server : IServer
    {
        private readonly ConcurrentDictionary<uint, ClientPeerTask> _clientPeerTasks = new();
        private readonly INetBufferPool _netBufferPool;
        private readonly ILogger? _logger;
        private int _isStarted;
        private uint _nextClientPeerId;

        public bool IsStarted => _isStarted == 1;
        public IList<IClientPeer> ClientPeers => _clientPeerTasks.Select(x => x.Value.Peer).ToList();

        public Server(INetBufferPool? netBufferPool, ILogger? logger)
        {
            _netBufferPool = netBufferPool ?? new NetBufferPool();
            _logger = logger;
        }

        public async Task RunAsync(IPEndPoint localEndpoint, CancellationToken cancellationToken = default)
        {
            ValidateAndSetStarted();

            using var tcpListener = CreateTcpListener(localEndpoint);
            tcpListener.Start();

            OnServerStarted();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await AcceptClientAsync(tcpListener, cancellationToken);
                }
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
                throw new InvalidOperationException("The server has been started");
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

        private ClientPeer CreateClientPeer(uint clientPeerId, TcpClient tcpClient)
        {
            return new ClientPeer(clientPeerId, tcpClient, _netBufferPool, _logger);
        }

        public IClientPeer? GetClientPeer(uint clientPeerId)
        {
            return _clientPeerTasks.GetValueOrDefault(clientPeerId)?.Peer;
        }

        protected abstract IClientPeerHandler CreateClientPeerHandler(IClientPeer clientPeer);
        protected virtual void OnListenerCreated(TcpListener tcpListener) { }
        protected virtual void OnClientAccepted(TcpClient tcpClient) { }
        protected virtual void OnClientPeerConnected(IClientPeer clientPeer) { }
        protected virtual void OnClientPeerDisconnected(IClientPeer clientPeer) { }
        protected virtual void OnServerStarted() { }

        private record ClientPeerTask(IClientPeer Peer, Task Task);
    }
}
