﻿using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Configurations;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using IRtmpServerStreamEventHandler = LiveStreamingServerNet.Rtmp.Server.Contracts.IRtmpServerStreamEventHandler;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal.Services
{
    internal class RtmpUpstreamManagerService : IRtmpUpstreamManagerService, IRtmpServerStreamEventHandler, IRtmpMediaMessageInterceptor, IAsyncDisposable
    {
        private readonly IServiceProvider _services;
        private readonly IRtmpUpstreamProcessFactory _upstreamProcessFactory;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly RtmpUpstreamConfiguration _config;

        private readonly ConcurrentDictionary<string, UpstreamProcessItem> _upstreamProcessTasks = new();
        private readonly object _syncLock = new();

        public RtmpUpstreamManagerService(
            IServiceProvider services,
            IRtmpUpstreamProcessFactory upstreamProcessFactory,
            IRtmpStreamManagerService streamManager,
            IOptions<RtmpUpstreamConfiguration> config)
        {
            _services = services;
            _upstreamProcessFactory = upstreamProcessFactory;
            _streamManager = streamManager;
            _config = config.Value;
        }

        private async ValueTask CreateUpstreamProcessIfNeededAsync(string streamPath)
        {
            if (!_config.Enabled || _upstreamProcessTasks.ContainsKey(streamPath) ||
                !await VerifyExtraConditionAsync(streamPath))
            {
                return;
            }

            lock (_syncLock)
            {
                if (_upstreamProcessTasks.ContainsKey(streamPath))
                    return;

                var publishStreamContext = _streamManager.GetPublishStreamContext(streamPath);
                if (publishStreamContext == null)
                    return;

                CreatetUpstreamProcessTask(publishStreamContext);
            }

            void CreatetUpstreamProcessTask(IRtmpPublishStreamContext publishStreamContext)
            {
                var cts = new CancellationTokenSource();
                var upstreamProcess = CreateUpstreamProcess(publishStreamContext);
                var upstreamProcessTask = UpstreamProcessTask(upstreamProcess, cts.Token);

                _upstreamProcessTasks[streamPath] = new(upstreamProcess, upstreamProcessTask, cts);

                _ = upstreamProcessTask.ContinueWith(async _ =>
                {
                    lock (_syncLock)
                    {
                        _upstreamProcessTasks.TryRemove(streamPath, out var _);
                        cts.Dispose();
                    }

                    await CreateUpstreamProcessIfNeededAsync(streamPath);
                });
            }
        }

        private async ValueTask<bool> VerifyExtraConditionAsync(string streamPath)
        {
            if (_config.Condition == null)
                return true;

            var publishStreamContext = _streamManager.GetPublishStreamContext(streamPath);
            if (publishStreamContext == null) return false;

            return await _config.Condition.ShouldRelayStreamAsync(_services, streamPath, publishStreamContext.StreamArguments);
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

        private IRtmpUpstreamProcess CreateUpstreamProcess(IRtmpPublishStreamContext publishStreamContext)
        {
            return _upstreamProcessFactory.Create(publishStreamContext);
        }

        private async Task UpstreamProcessTask(IRtmpUpstreamProcess upstreamProcess, CancellationToken cancellationToken)
        {
            await upstreamProcess.RunAsync(cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public bool FilterMediaMessage(string streamPath, MediaType mediaType, uint timestamp, bool isSkippable)
        {
            return _upstreamProcessTasks.ContainsKey(streamPath);
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
            if (clientId == 0)
                return ValueTask.CompletedTask;

            return CreateUpstreamProcessIfNeededAsync(streamPath);
        }

        public ValueTask OnRtmpStreamUnpublishedAsync(IEventContext context, uint clientId, string streamPath)
        {
            if (clientId == 0)
                return ValueTask.CompletedTask;

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
