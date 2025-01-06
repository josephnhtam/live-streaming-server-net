using LiveStreamingServerNet.KubernetesPod.Configurations;
using LiveStreamingServerNet.KubernetesPod.Internal.Logging;
using LiveStreamingServerNet.KubernetesPod.Internal.Services.Contracts;
using LiveStreamingServerNet.KubernetesPod.StreamRegistration;
using LiveStreamingServerNet.KubernetesPod.StreamRegistration.Contracts;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Networking.Server.Contracts;
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
            ISessionInfo client, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
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
                    _logger.StreamRegistrationFailed(client.Id, streamPath, result.Reason ?? "Unknown");
                    _streamContexts.TryRemove(streamPath, out var _);
                    return result;
                }

                _logger.StreamRegistered(client.Id, streamPath);
                CreateKeepaliveTask(context);

                return result;
            }
            catch (Exception ex)
            {
                _logger.RegisteringStreamError(client.Id, streamPath, ex);
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
                _logger.StreamUnregistered(context.Client.Id, streamPath);
            }
            catch (Exception ex)
            {
                _logger.StreamUnregistrationFailed(context.Client.Id, streamPath, ex);
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
            _ = task.ContinueWith(_ =>
                _streamKeepaliveTasks.TryRemove(context, out var _), TaskContinuationOptions.ExecuteSynchronously);
        }

        private async Task StreamKeepaliveTask(StreamContext context)
        {
            _logger.KeepaliveTaskStarted(context.Client.Id, context.StreamPath);

            var cancellationToken = context.KeepaliveCancellationToken;
            var lastRevalidationTime = DateTime.UtcNow;
            var isRetrying = false;

            while (!cancellationToken.IsCancellationRequested)
            {
                var delay = isRetrying ? _config.KeepaliveRetryDelay : _config.KeepaliveInterval;

                if (DateTime.UtcNow + delay > lastRevalidationTime + _config.KeepaliveTimeout)
                {
                    _logger.RevalidatingStreamTimedOut(context.Client.Id, context.StreamPath);
                    break;
                }

                try
                {
                    await Task.Delay(delay, cancellationToken);

                    var revlidationTime = DateTime.UtcNow;
                    var result = await _streamStore.RevalidateStreamAsync(context.StreamPath, cancellationToken);

                    if (result.Successful)
                    {
                        _logger.StreamRevalidated(context.Client.Id, context.StreamPath, revlidationTime);
                        isRetrying = false;
                        lastRevalidationTime = revlidationTime;
                    }
                    else
                    {
                        _logger.RevalidatingStreamFailed(context.Client.Id, context.StreamPath, result.Retryable, result.Reason ?? "Unknown");
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
                    _logger.RevalidatingStreamError(context.Client.Id, context.StreamPath, ex);
                    isRetrying = true;
                }
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                _logger.DisconnectingClientDueToKeepaliveFailure(context.Client.Id, context.StreamPath);
                _server.GetClient(context.Client.Id)?.Disconnect();
            }

            _logger.KeepaliveTaskStopped(context.Client.Id, context.StreamPath);
        }

        public async ValueTask DisposeAsync()
        {
            await Task.WhenAll(_streamKeepaliveTasks.Values);
        }

        private class StreamContext
        {
            public ISessionInfo Client { get; }
            public string StreamPath { get; }
            public IReadOnlyDictionary<string, string> StreamArguments { get; }
            public CancellationToken KeepaliveCancellationToken { get; }

            private readonly CancellationTokenSource _keepaliveCts;

            public StreamContext(ISessionInfo client, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
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
