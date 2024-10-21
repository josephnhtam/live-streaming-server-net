using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Networking.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Configurations;
using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Exceptions;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Client.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Utilities.Contracts;
using LiveStreamingServerNet.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal sealed class RtmpClient : IRtmpClient, IRtmpHandshakeEventHandler
    {
        private readonly IClient _client;
        private readonly IRtmpClientContext _context;
        private readonly IRtmpCommanderService _commander;
        private readonly IRtmpProtocolControlService _protocolControl;
        private readonly IRtmpStreamFactory _streamFactory;
        private readonly ILogger<RtmpClient> _logger;
        private readonly RtmpClientConfiguration _config;

        private readonly CancellationTokenSource _clientCts = new();
        private readonly TaskCompletionSource _clientTcs = new();
        private readonly TaskCompletionSource _handshakeTcs = new();

        private int _connectOnce;
        private bool _connected;
        private Task? _clientTask;
        private bool _isDisposed;

        public IServiceProvider Services => _client.Services;
        public RtmpBandwidthLimit? BandwidthLimit { get; private set; }

        public event EventHandler<BandwidthLimitEventArgs>? OnBandwidthLimitUpdated;

        public RtmpClient(
            IClient client,
            IRtmpClientContext context,
            IRtmpCommanderService commander,
            IRtmpProtocolControlService protocolControl,
            IRtmpStreamFactory streamFactory,
            IOptions<RtmpClientConfiguration> config,
            ILogger<RtmpClient> logger)
        {
            _client = client;
            _context = context;
            _commander = commander;
            _protocolControl = protocolControl;
            _streamFactory = streamFactory;
            _config = config.Value;
            _logger = logger;
        }

        public RtmpClientStatus Status => this switch
        {
            _ when _clientTcs.Task.IsCompleted => RtmpClientStatus.Stopped,
            _ when _connected => RtmpClientStatus.Connected,
            _ when _handshakeTcs.Task.IsCompletedSuccessfully => RtmpClientStatus.Connecting,
            _ when _clientTask != null => RtmpClientStatus.Handshaking,
            _ => RtmpClientStatus.None
        };

        private void OnSessionContextReady()
        {
            Debug.Assert(_context.SessionContext != null);

            _context.SessionContext.OnBandwidthLimitUpdated += OnSessionContextBandwidthLimitUpdated;
        }

        public Task<ConnectResponse> ConnectAsync(ServerEndPoint endPoint, string appName)
        {
            return ConnectAsync(endPoint, appName, new Dictionary<string, object>());
        }

        public async Task<ConnectResponse> ConnectAsync(ServerEndPoint endPoint, string appName, IDictionary<string, object> information)
        {
            if (Interlocked.CompareExchange(ref _connectOnce, 1, 0) == 1)
                throw new InvalidOperationException("Connect method can be called only once.");

            try
            {
                _clientTask = RunClientAsync(endPoint);
                return await ConnectAfterHandshakeAsync(appName, information);
            }
            catch
            {
                _clientCts.Cancel();
                throw;
            }
        }

        public async Task<IRtmpStream> CreateStreamAsync()
        {
            if (!_connected)
                throw new InvalidOperationException("Client is not connected.");

            var createStreamTcs = new TaskCompletionSource<IRtmpStream>();

            _commander.CreateStream(
                callback: (success, streamContext) =>
                {
                    if (success && streamContext != null)
                    {
                        var stream = _streamFactory.Create(streamContext);
                        createStreamTcs.TrySetResult(stream);
                    }
                    else
                    {
                        createStreamTcs.TrySetException(new RtmpClientCommandException("Create stream failed."));
                    }

                    return ValueTask.CompletedTask;
                },
                cancellationCallback: () =>
                    createStreamTcs.TrySetException(new RtmpClientCommandException("Create stream failed."))
            );

            return await createStreamTcs.Task;
        }

        private async Task RunClientAsync(ServerEndPoint endPoint)
        {
            try
            {
                await _client.RunAsync(endPoint, _clientCts.Token);
                _clientTcs.TrySetResult();
            }
            catch (Exception ex)
            {
                _clientTcs.TrySetException(ex);
            }
            finally
            {
                _connected = false;
            }
        }

        private async Task<ConnectResponse> ConnectAfterHandshakeAsync(string appName, IDictionary<string, object> connectInformation)
        {
            await AwaitForHandshakeAsync();

            var connectTcs = new TaskCompletionSource<ConnectResponse>();

            _protocolControl.SetChunkSize(_config.OutChunkSize);
            _protocolControl.WindowAcknowledgementSize(_config.WindowAcknowledgementSize);

            _commander.Connect(appName, connectInformation,
                callback: (success, information, parameters) =>
                {
                    if (success)
                    {
                        _connected = true;
                        connectTcs.TrySetResult(new(new Dictionary<string, object>(information), parameters));
                    }
                    else
                    {
                        connectTcs.TrySetException(new RtmpClientConnectionException());
                    }

                    return ValueTask.CompletedTask;
                },
                cancellationCallback: () =>
                    connectTcs.TrySetException(new RtmpClientConnectionException())
            );

            return await connectTcs.Task;
        }

        public void Command(RtmpCommand command)
        {
            _commander.Command(command.ToMessage(RtmpConstants.ControlStreamId, RtmpConstants.ControlChunkStreamId));
        }

        public async Task<RtmpCommandResponse> CommandAsync(RtmpCommand command)
        {
            var tcs = new TaskCompletionSource<RtmpCommandResponse>();

            _commander.Command(
                command.ToMessage(RtmpConstants.ControlStreamId, RtmpConstants.ControlChunkStreamId),
                callback: (context, response) =>
                {
                    tcs.SetResult(response);
                    return Task.FromResult(true);
                },
                cancellationCallback: () => tcs.TrySetCanceled()
            );

            return await tcs.Task;
        }

        private async Task AwaitForHandshakeAsync()
        {
            var timeoutTask = Task.Delay(_config.HandshakeTimeout, _clientCts.Token);
            var completedTask = await Task.WhenAny(_handshakeTcs.Task, _clientTcs.Task, timeoutTask);

            if (completedTask.IsCanceled)
            {
                throw new TaskCanceledException();
            }
            else if (completedTask == timeoutTask)
            {
                throw new TimeoutException("Handshake timeout.");
            }
            else if (completedTask == _clientTcs.Task)
            {
                throw new RtmpClientConnectionException("Client connection failed.", completedTask.Exception);
            }

            await completedTask;
        }

        public void Stop()
        {
            try
            {
                _clientCts.Cancel();
            }
            catch (ObjectDisposedException) { }
        }

        public async Task UntilStoppedAsync(CancellationToken cancellationToken)
        {
            if (_clientTask != null)
            {
                await _clientTcs.Task.WithCancellation(cancellationToken);
            }
        }

        private void OnSessionContextBandwidthLimitUpdated(object? sender, BandwidthLimitEventArgs e)
        {
            try
            {
                BandwidthLimit = e.BandwidthLimit;
                OnBandwidthLimitUpdated?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                _logger.BandwidthLimitUpdateError(ex);
            }
        }

        ValueTask IRtmpHandshakeEventHandler.OnRtmpHandshakeCompleteAsync(IEventContext context)
        {
            OnSessionContextReady();
            _handshakeTcs.TrySetResult();
            return ValueTask.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (!_clientCts.IsCancellationRequested)
            {
                _clientCts.Cancel();
            }

            if (_clientTask != null)
            {
                await _clientTask;
            }

            _clientCts.Dispose();
        }
    }
}
