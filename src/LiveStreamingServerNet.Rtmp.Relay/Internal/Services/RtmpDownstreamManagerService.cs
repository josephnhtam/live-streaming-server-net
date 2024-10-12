using LiveStreamingServerNet.Rtmp.Relay.Configurations;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal.Services
{
    internal class RtmpDownstreamManagerService : IRtmpDownstreamManagerService, IRtmpServerStreamEventHandler, IAsyncDisposable
    {
        private readonly IServiceProvider _services;
        private readonly IRtmpDownstreamProcessFactory _downstreamProcessFactory;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly RtmpDownstreamConfiguration _config;

        private readonly Dictionary<string, DownstreamProcessItem> _downstreamProcessTasks = new();
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

        private void CreateDownstreamProcessIfNeeded(string streamPath)
        {
            if (!_config.Enabled)
                return;

            lock (_syncLock)
            {
                if (_downstreamProcessTasks.ContainsKey(streamPath))
                    return;

                if (!_streamManager.IsStreamBeingSubscribed(streamPath) ||
                    _streamManager.IsStreamPublishing(streamPath))
                    return;

                CreatetDownstreamProcessTask(streamPath);
            }

            void CreatetDownstreamProcessTask(string streamPath)
            {
                var cts = new CancellationTokenSource();
                var downstreamProcessTask = DownstreamProcessTask(streamPath, cts.Token);

                _downstreamProcessTasks[streamPath] = new(downstreamProcessTask, cts);

                _ = downstreamProcessTask.ContinueWith(_ =>
                {
                    lock (_syncLock)
                    {
                        _downstreamProcessTasks.Remove(streamPath);
                        cts.Dispose();

                        CreateDownstreamProcessIfNeeded(streamPath);
                    }
                });
            }
        }

        private void RemoveDownstreamProcessIfNeeded(string streamPath)
        {
            lock (_syncLock)
            {
                if (!_downstreamProcessTasks.TryGetValue(streamPath, out var downstreamProcessTaskItem))
                    return;

                if (_streamManager.IsStreamBeingSubscribed(streamPath))
                    return;

                downstreamProcessTaskItem.Cts.Cancel();
            }
        }

        private async Task DownstreamProcessTask(string streamPath, CancellationToken cancellationToken)
        {
            var downstreamProcess = _downstreamProcessFactory.Create(streamPath);
            await downstreamProcess.RunAsync(cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            var downstreamProcesses = _downstreamProcessTasks.Values.ToArray();

            foreach (var downstreamProcess in downstreamProcesses)
            {
                downstreamProcess.Cts.Cancel();
            }

            await Task.WhenAll(downstreamProcesses.Select(x => x.DownsteramProcessTask));
        }

        public ValueTask OnRtmpStreamSubscribedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            CreateDownstreamProcessIfNeeded(streamPath);
            return ValueTask.CompletedTask;
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
    }
}
