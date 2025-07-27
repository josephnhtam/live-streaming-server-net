using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using LiveStreamingServerNet.Networking.Server.Internal.Contracts;
using LiveStreamingServerNet.Networking.Server.Internal.Logging;
using LiveStreamingServerNet.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Networking.Server.Internal
{
    internal class ClientSessionManager : IClientSessionManager
    {
        private readonly ConcurrentDictionary<uint, ISession> _clientSessions = new();
        private readonly ConcurrentDictionary<uint, ClientSessionTask> _clientSessionTasks = new();
        private readonly ISessionFactory _clientSessionFactory;
        private readonly IServerEventDispatcher _eventDispatcher;
        private readonly ILogger _logger;

        private uint _nextClientId;

        public ClientSessionManager(
            ISessionFactory clientSessionFactory,
            IServerEventDispatcher eventDispatcher,
            ILogger<ClientSessionManager> logger)
        {
            _clientSessionFactory = clientSessionFactory;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        public async Task AcceptClientAsync(ITcpListenerInternal tcpListener, ServerEndPoint serverEndPoint, CancellationToken cancellationToken)
        {
            var tcpClient = await tcpListener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);

            await OnClientAcceptedAsync(tcpClient).ConfigureAwait(false);

            var clientId = GetNextClientId();
            var clientSession = CreateClientSession(clientId, tcpClient, serverEndPoint);

            _logger.ClientConnected(clientId);

            _clientSessions.TryAdd(clientId, clientSession);
            var clientSessionTask = clientSession.RunAsync(cancellationToken);

            _clientSessionTasks.TryAdd(clientId, new(clientSession, clientSessionTask));
            await OnClientConnectedAsync(clientSession).ConfigureAwait(false);

            _ = clientSessionTask.ContinueWith(async _ =>
            {
                await OnClientDisconnected(clientSession).ConfigureAwait(false);
                _clientSessions.TryRemove(clientId, out var _);
                _clientSessionTasks.TryRemove(clientId, out var _);

                await clientSession.DisposeAsync().ConfigureAwait(false);
                _logger.ClientDisconnected(clientId);
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        private uint GetNextClientId()
        {
            return Interlocked.Increment(ref _nextClientId);
        }

        private ISession CreateClientSession(uint clientId, ITcpClientInternal tcpClient, ServerEndPoint serverEndPoint)
        {
            return _clientSessionFactory.Create(clientId, tcpClient, serverEndPoint);
        }

        public ISessionHandle? GetClient(uint clientId)
        {
            return _clientSessions.GetValueOrDefault(clientId);
        }

        public IReadOnlyList<ISessionHandle> GetClients()
        {
            return _clientSessions.Select(x => x.Value).OfType<ISessionHandle>().ToList();
        }

        public async Task WaitUntilAllClientTasksCompleteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.WhenAll(_clientSessionTasks.Select(x => x.Value.Task)).WithCancellation(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        }

        private async Task OnClientAcceptedAsync(ITcpClient tcpClient)
        {
            await _eventDispatcher.ClientAcceptedAsync(tcpClient).ConfigureAwait(false);
        }

        private async Task OnClientConnectedAsync(ISessionHandle client)
        {
            await _eventDispatcher.ClientConnectedAsync(client).ConfigureAwait(false);
        }

        private async Task OnClientDisconnected(ISessionInfo client)
        {
            await _eventDispatcher.ClientDisconnectedAsync(client).ConfigureAwait(false);
        }

        private record ClientSessionTask(ISession Client, Task Task);
    }
}
