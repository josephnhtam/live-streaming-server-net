using LiveStreamingServerNet.KubernetesPod.Configurations;
using LiveStreamingServerNet.KubernetesPod.Internal.Logging;
using LiveStreamingServerNet.KubernetesPod.Internal.Services.Contracts;
using LiveStreamingServerNet.KubernetesPod.StreamRegistration;
using LiveStreamingServerNet.KubernetesPod.StreamRegistration.Contracts;
using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.KubernetesPod.Internal.Services
{
    internal class StreamRegistry : IStreamRegistry, IAsyncDisposable
    {
        private readonly IStreamStore _streamStore;
        private readonly IServer _server;
        private readonly IKubernetesContext _kubernetesContext;
        private readonly StreamRegistryConfiguration _config;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, StreamContext> _streamContexts;
        private readonly ConcurrentDictionary<StreamContext, Task> _streamKeepaliveTasks;

        public StreamRegistry(
            IServer server,
            IKubernetesContext kubernetesContext,
            IOptions<StreamRegistryConfiguration> config,
            ILogger<StreamRegistry> logger,
            IStreamStore? streamStore = null)
        {
            _streamStore = streamStore ?? throw new ArgumentNullException("No stream store is registered");
            _server = server;
            _kubernetesContext = kubernetesContext;
            _config = config.Value;
            _logger = logger;

            _streamContexts = new ConcurrentDictionary<string, StreamContext>();
            _streamKeepaliveTasks = new ConcurrentDictionary<StreamContext, Task>();
        }

        public async Task<StreamRegistrationResult> RegisterStreamAsync(
            IClientInfo client, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var context = new StreamContext(client, streamPath, streamArguments);

            if (!_streamContexts.TryAdd(streamPath, context))
                return StreamRegistrationResult.Failure("Stream is already registered.");

            try
            {
                var result = await _streamStore.RegisterStreamAsync(
                    client,
                    _kubernetesContext.PodNamespace,
                    _kubernetesContext.PodName,
                    streamPath,
                    streamArguments);

                if (!result.Successful)
                {
                    _logger.StreamRegistrationFailed(client.ClientId, streamPath, result.Reason ?? "Unknown");
                    _streamContexts.TryRemove(streamPath, out var _);
                    return result;
                }

                _logger.StreamRegistered(client.ClientId, streamPath);
                CreateKeepaliveTask(context);

                return result;
            }
            catch (Exception ex)
            {
                _logger.RegisteringStreamError(client.ClientId, streamPath, ex);
                _streamContexts.TryRemove(streamPath, out var _);
                return StreamRegistrationResult.Failure("An error occurred during registering the stream.");
            }
        }

        public async Task UnregsiterStreamAsync(string streamPath)
        {
            if (!_streamContexts.TryGetValue(streamPath, out var context))
                return;

            context.StopKeepalive();

            try
            {
                await _streamStore.UnregsiterStreamAsync(streamPath);
                _logger.StreamUnregistered(context.Client.ClientId, streamPath);
            }
            catch (Exception ex)
            {
                _logger.StreamUnregistrationFailed(context.Client.ClientId, streamPath, ex);
            }
            finally
            {
                _streamContexts.TryRemove(streamPath, out var _);
            }
        }

        public async Task<bool> IsStreamRegisteredAsync(string streamPath, bool checkLocalOnly)
        {
            if (_streamContexts.ContainsKey(streamPath))
                return true;

            if (checkLocalOnly)
                return false;

            return await _streamStore.IsStreamRegisteredAsync(streamPath);
        }

        private void CreateKeepaliveTask(StreamContext context)
        {
            var streamPath = context.StreamPath;

            var task = Task.Run(() => StreamKeepaliveTask(context));
            _streamKeepaliveTasks[context] = task;
            _ = task.ContinueWith(_ => _streamKeepaliveTasks.TryRemove(context, out var _));
        }

        private async Task StreamKeepaliveTask(StreamContext context)
        {
            _logger.KeepaliveTaskStarted(context.Client.ClientId, context.StreamPath);

            var cancellationToken = context.KeepaliveCancellationToken;
            var lastRevalidationTime = DateTime.UtcNow;
            var isRetrying = false;

            while (!cancellationToken.IsCancellationRequested)
            {
                var delay = isRetrying ? _config.KeepaliveRetryDelay : _config.KeepaliveInterval;

                if (DateTime.UtcNow + delay > lastRevalidationTime + _config.KeepaliveTimeout)
                {
                    _logger.RevalidatingStreamTimedOut(context.Client.ClientId, context.StreamPath);
                    break;
                }

                try
                {
                    await Task.Delay(delay, cancellationToken);

                    var revlidationTime = DateTime.UtcNow;
                    var result = await _streamStore.RevalidateStreamAsync(context.StreamPath, cancellationToken);

                    if (result.Successful)
                    {
                        _logger.StreamRevalidated(context.Client.ClientId, context.StreamPath, revlidationTime);
                        isRetrying = false;
                        lastRevalidationTime = revlidationTime;
                    }
                    else
                    {
                        _logger.RevalidatingStreamFailed(context.Client.ClientId, context.StreamPath, result.Retryable, result.Reason ?? "Unknown");
                        isRetrying = result.Retryable;

                        if (!isRetrying)
                            break;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.RevalidatingStreamError(context.Client.ClientId, context.StreamPath, ex);
                    isRetrying = true;
                }
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                _logger.DisconnectingClientDueToKeepaliveFailure(context.Client.ClientId, context.StreamPath);
                _server.GetClient(context.Client.ClientId)?.Disconnect();
            }

            _logger.KeepaliveTaskStopped(context.Client.ClientId, context.StreamPath);
        }

        public async ValueTask DisposeAsync()
        {
            await Task.WhenAll(_streamKeepaliveTasks.Values);
        }

        private class StreamContext
        {
            public IClientInfo Client { get; }
            public string StreamPath { get; }
            public IReadOnlyDictionary<string, string> StreamArguments { get; }
            public CancellationToken KeepaliveCancellationToken { get; }

            private readonly CancellationTokenSource _keepaliveCts;

            public StreamContext(IClientInfo client, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            {
                Client = client;
                StreamPath = streamPath;
                StreamArguments = new Dictionary<string, string>(streamArguments);

                _keepaliveCts = new CancellationTokenSource();
                KeepaliveCancellationToken = _keepaliveCts.Token;
            }

            public void StopKeepalive()
            {
                _keepaliveCts.Cancel();
            }
        }
    }
}
