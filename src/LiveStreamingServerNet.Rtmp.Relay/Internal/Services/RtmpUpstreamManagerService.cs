using LiveStreamingServerNet.Rtmp.Relay.Configurations;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal.Services
{
    internal class RtmpUpstreamManagerService : IRtmpUpstreamManagerService, IRtmpServerStreamEventHandler, IRtmpMediaMessageInterceptor, IAsyncDisposable
    {
        private readonly IRtmpUpstreamProcessFactory _upstreamProcessFactory;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly RtmpUpstreamConfiguration _config;

        private readonly ConcurrentDictionary<string, UpstreamProcessItem> _upstreamProcessTasks = new();
        private readonly object _syncLock = new();

        public RtmpUpstreamManagerService(
            IRtmpUpstreamProcessFactory upstreamProcessFactory,
            IRtmpStreamManagerService streamManager,
            IOptions<RtmpUpstreamConfiguration> config)
        {
            _upstreamProcessFactory = upstreamProcessFactory;
            _streamManager = streamManager;
            _config = config.Value;
        }

        private void CreateUpstreamProcessIfNeeded(string streamPath)
        {
            if (!_config.Enabled)
                return;

            lock (_syncLock)
            {
                if (_upstreamProcessTasks.ContainsKey(streamPath))
                    return;

                if (!_streamManager.IsStreamPublishing(streamPath))
                    return;

                CreatetUpstreamProcessTask(streamPath);
            }

            void CreatetUpstreamProcessTask(string streamPath)
            {
                var cts = new CancellationTokenSource();
                var upstreamProcess = CreateUpstreamProcess(streamPath);
                var upstreamProcessTask = UpstreamProcessTask(upstreamProcess, cts.Token);

                _upstreamProcessTasks[streamPath] = new(upstreamProcess, upstreamProcessTask, cts);

                _ = upstreamProcessTask.ContinueWith(_ =>
                {
                    lock (_syncLock)
                    {
                        _upstreamProcessTasks.TryRemove(streamPath, out var _);
                        cts.Dispose();

                        CreateUpstreamProcessIfNeeded(streamPath);
                    }
                });
            }
        }

        private void RemoveUpstreamProcessIfNeeded(string streamPath)
        {
            lock (_syncLock)
            {
                if (!_upstreamProcessTasks.TryGetValue(streamPath, out var upstreamProcessTaskItem))
                    return;

                if (_streamManager.IsStreamPublishing(streamPath))
                    return;

                upstreamProcessTaskItem.Cts.Cancel();
            }
        }

        private IRtmpUpstreamProcess CreateUpstreamProcess(string streamPath)
        {
            return _upstreamProcessFactory.Create(streamPath);
        }

        private async Task UpstreamProcessTask(IRtmpUpstreamProcess upstreamProcess, CancellationToken cancellationToken)
        { 
            await upstreamProcess.RunAsync(cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask OnReceiveMediaMessageAsync(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable)
        {
            if (_upstreamProcessTasks.TryGetValue(streamPath, out var upstreamProcessTaskItem))
            {
                upstreamProcessTaskItem.UpstreamProcess.OnReceiveMediaData(mediaType, rentedBuffer, timestamp, isSkippable);
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpStreamPublishedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            CreateUpstreamProcessIfNeeded(streamPath);
            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpStreamUnpublishedAsync(IEventContext context, uint clientId, string streamPath)
        {
            RemoveUpstreamProcessIfNeeded(streamPath);
            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpStreamSubscribedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpStreamUnsubscribedAsync(IEventContext context, uint clientId, string streamPath)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpStreamMetaDataReceivedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData)
            => ValueTask.CompletedTask;

        private record UpstreamProcessItem(IRtmpUpstreamProcess UpstreamProcess, Task UpsteramProcessTask, CancellationTokenSource Cts);
    }
}
