using LiveStreamingServerNet.Rtmp.Relay.Configurations;
using LiveStreamingServerNet.Rtmp.Relay.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Streams.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal.Services
{
    internal class RtmpDownstreamManagerService : IRtmpDownstreamManagerService, IRtmpServerStreamEventHandler, IAsyncDisposable
    {
        private readonly IServiceProvider _services;
        private readonly IRtmpDownstreamProcessFactory _downstreamProcessFactory;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly RtmpDownstreamConfiguration _config;

        private readonly ConcurrentDictionary<string, DownstreamProcessItem> _downstreamProcessTasks = new();
        private readonly ConcurrentDictionary<string, List<IRtmpDownstreamSubscriber>> _downstreamSubscribers = new();
        private readonly object _syncLock = new();

        public RtmpDownstreamManagerService(
            IServiceProvider services,
            IRtmpDownstreamProcessFactory downstreamProcessFactory,
            IRtmpStreamManagerService streamManager,
            IOptions<RtmpDownstreamConfiguration> config)
        {
            _services = services;
            _downstreamProcessFactory = downstreamProcessFactory;
            _streamManager = streamManager;
            _config = config.Value;
        }

        public async Task<IRtmpDownstreamSubscriber?> RequestDownstreamAsync(string streamPath)
        {
            var subscriber = CreateDownstreamSubscriber(streamPath);

            try
            {
                var result = await CreateDownstreamProcessIfNeededAsync(streamPath).ConfigureAwait(false);

                if (result == CreateDownstreamProcessResult.NotCreated)
                    throw new Exception("Downstream process was not created.");

                return subscriber;
            }
            catch
            {
                subscriber.Dispose();
                return null;
            }
        }

        public bool IsDownstreamRequested(string streamPath)
        {
            lock (_syncLock)
            {
                return _downstreamSubscribers.ContainsKey(streamPath);
            }
        }

        private IRtmpDownstreamSubscriber CreateDownstreamSubscriber(string streamPath)
        {
            lock (_syncLock)
            {
                if (!_downstreamSubscribers.TryGetValue(streamPath, out var subscribers))
                {
                    subscribers = new List<IRtmpDownstreamSubscriber>();
                    _downstreamSubscribers[streamPath] = subscribers;
                }

                var subscriber = new RtmpDownstreamSubscriber(streamPath, RemoveDownstreamSubscriber);
                subscribers.Add(subscriber);

                return subscriber;
            }
        }

        private void RemoveDownstreamSubscriber(IRtmpDownstreamSubscriber subscriber)
        {
            lock (_syncLock)
            {
                if (_downstreamSubscribers.TryGetValue(subscriber.StreamPath, out var subscribers))
                {
                    subscribers.Remove(subscriber);

                    if (subscribers.Count == 0)
                    {
                        _downstreamSubscribers.TryRemove(subscriber.StreamPath, out _);
                        RemoveDownstreamProcessIfNeeded(subscriber.StreamPath);
                    }
                }
            }
        }

        private async ValueTask<CreateDownstreamProcessResult> CreateDownstreamProcessIfNeededAsync(string streamPath)
        {
            if (!_config.Enabled)
                return CreateDownstreamProcessResult.NotCreated;

            if (IsDownstreamProcessActive(streamPath))
                return CreateDownstreamProcessResult.AlreadyExists;

            if (!IsDownstreamNeeded(streamPath))
                return CreateDownstreamProcessResult.NotCreated;

            if (!await CheckExtraConditionAsync(streamPath).ConfigureAwait(false))
                return CreateDownstreamProcessResult.NotCreated;

            var tcs = new TaskCompletionSource<CreateDownstreamProcessResult>();

            lock (_syncLock)
            {
                if (IsDownstreamProcessActive(streamPath))
                    return CreateDownstreamProcessResult.AlreadyExists;

                if (!IsDownstreamNeeded(streamPath))
                    return CreateDownstreamProcessResult.NotCreated;

                CreatetDownstreamProcessTask(streamPath, tcs);
            }

            return await tcs.Task.ConfigureAwait(false);

            void CreatetDownstreamProcessTask(string streamPath, TaskCompletionSource<CreateDownstreamProcessResult> tcs)
            {
                var cts = new CancellationTokenSource();

                var downstreamProcess = CreateDownstreamProcess(streamPath);
                var downstreamProcessTask = DownstreamProcessTask(downstreamProcess, tcs, cts.Token);

                _downstreamProcessTasks[streamPath] = new(downstreamProcessTask, cts);
                _ = downstreamProcessTask.ContinueWith(_ =>
                    FinalizeDownstreamProcessAsync(downstreamProcess, streamPath, cts),
                    TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        private async Task FinalizeDownstreamProcessAsync(IRtmpDownstreamProcess downstreamProcess, string streamPath, CancellationTokenSource cts)
        {
            await downstreamProcess.DisposeAsync().ConfigureAwait(false);
            cts.Dispose();

            lock (_syncLock)
            {
                _downstreamProcessTasks.TryRemove(streamPath, out var _);
            }

            await CreateDownstreamProcessIfNeededAsync(streamPath).ConfigureAwait(false);
        }

        private bool IsDownstreamProcessActive(string streamPath)
        {
            return _downstreamProcessTasks.ContainsKey(streamPath);
        }

        private bool IsDownstreamNeeded(string streamPath)
        {
            if (_streamManager.IsStreamPublishing(streamPath))
                return false;

            if (!_streamManager.IsStreamBeingSubscribed(streamPath) &&
                !_downstreamSubscribers.ContainsKey(streamPath))
                return false;

            return true;
        }

        private async ValueTask<bool> CheckExtraConditionAsync(string streamPath)
        {
            if (_config.Condition == null)
                return true;

            return await _config.Condition.ShouldRelayStreamAsync(_services, streamPath).ConfigureAwait(false);
        }

        private void RemoveDownstreamProcessIfNeeded(string streamPath)
        {
            lock (_syncLock)
            {
                if (!_downstreamProcessTasks.TryGetValue(streamPath, out var downstreamProcessTaskItem))
                    return;

                if (_streamManager.IsStreamBeingSubscribed(streamPath))
                    return;

                if (_downstreamSubscribers.ContainsKey(streamPath))
                    return;

                downstreamProcessTaskItem.Cts.Cancel();
            }
        }

        private IRtmpDownstreamProcess CreateDownstreamProcess(string streamPath)
        {
            return _downstreamProcessFactory.Create(streamPath);
        }

        private async Task<bool> InitializeDownstreamProcessAsync(
            IRtmpDownstreamProcess downstreamProcess, TaskCompletionSource<CreateDownstreamProcessResult> tcs, CancellationToken cancellationToken)
        {
            try
            {
                var publishingStreamResult = await downstreamProcess.InitializeAsync(cancellationToken).ConfigureAwait(false);

                switch (publishingStreamResult)
                {
                    case PublishingStreamResult.Succeeded:
                        tcs.TrySetResult(CreateDownstreamProcessResult.Created);
                        return true;

                    case PublishingStreamResult.AlreadyExists:
                        tcs.TrySetResult(CreateDownstreamProcessResult.AlreadyExists);
                        return false;

                    default:
                        tcs.TrySetResult(CreateDownstreamProcessResult.NotCreated);
                        return false;
                }
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
                return false;
            }
        }

        private async Task DownstreamProcessTask(
            IRtmpDownstreamProcess downstreamProcess, TaskCompletionSource<CreateDownstreamProcessResult> tcs, CancellationToken cancellationToken)
        {
            if (!await InitializeDownstreamProcessAsync(downstreamProcess, tcs, cancellationToken).ConfigureAwait(false))
                return;

            await downstreamProcess.RunAsync(cancellationToken).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            var downstreamProcesses = _downstreamProcessTasks.Values.ToArray();

            foreach (var downstreamProcess in downstreamProcesses)
            {
                downstreamProcess.Cts.Cancel();
            }

            await Task.WhenAll(downstreamProcesses.Select(x => x.DownsteramProcessTask)).ConfigureAwait(false);
        }

        public async ValueTask OnRtmpStreamSubscribedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            await CreateDownstreamProcessIfNeededAsync(streamPath).ConfigureAwait(false);
        }

        public ValueTask OnRtmpStreamUnsubscribedAsync(IEventContext context, uint clientId, string streamPath)
        {
            RemoveDownstreamProcessIfNeeded(streamPath);
            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpStreamMetaDataReceivedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpStreamPublishedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpStreamUnpublishedAsync(IEventContext context, uint clientId, string streamPath)
            => ValueTask.CompletedTask;

        private record DownstreamProcessItem(Task DownsteramProcessTask, CancellationTokenSource Cts);

        private enum CreateDownstreamProcessResult
        {
            Created,
            AlreadyExists,
            NotCreated
        }
    }
}
