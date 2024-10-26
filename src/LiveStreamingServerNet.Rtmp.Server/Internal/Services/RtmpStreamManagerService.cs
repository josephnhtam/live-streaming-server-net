using LiveStreamingServerNet.Rtmp.Server.Configurations;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Extensions;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpStreamManagerService : IRtmpStreamManagerService
    {
        private readonly object _publishingSyncLock = new();
        private readonly Dictionary<string, IRtmpPublishStreamContext> _publishStreamContexts = new();
        private readonly Dictionary<string, PublishStreamContinuationContext> _publishStreamContinuationContexts = new();

        private readonly object _subscribingSyncLock = new();
        private readonly Dictionary<string, List<IRtmpSubscribeStreamContext>> _subscribeStreamContexts = new();

        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpUserControlMessageSenderService _userControlMessageSender;
        private readonly IRtmpServerStreamEventDispatcher _eventDispatcher;
        private readonly RtmpServerConfiguration _config;

        public RtmpStreamManagerService(
            IRtmpCommandMessageSenderService commandMessageSender,
            IRtmpUserControlMessageSenderService userControlMessageSender,
            IRtmpServerStreamEventDispatcher eventDispatcher,
            IOptions<RtmpServerConfiguration> config)
        {
            _commandMessageSender = commandMessageSender;
            _userControlMessageSender = userControlMessageSender;
            _eventDispatcher = eventDispatcher;
            _config = config.Value;
        }

        private (PublishingStreamResult Result, IList<IRtmpSubscribeStreamContext> SubscribeStreamContexts) StartPublishing(
            IRtmpStreamContext streamContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    if (streamContext.PublishContext != null)
                    {
                        SendBadPublishingConnectionMessage(streamContext, "Already publishing.");
                        return (PublishingStreamResult.AlreadyPublishing, new List<IRtmpSubscribeStreamContext>());
                    }

                    if (streamContext.SubscribeContext != null)
                    {
                        SendBadPublishingConnectionMessage(streamContext, "Already subscribing.");
                        return (PublishingStreamResult.AlreadySubscribing, new List<IRtmpSubscribeStreamContext>());
                    }

                    if (_publishStreamContexts.ContainsKey(streamPath))
                    {
                        SendAlreadyExistsMessage(streamContext);
                        return (PublishingStreamResult.AlreadyExists, new List<IRtmpSubscribeStreamContext>());
                    }

                    var publishStreamContext = streamContext.CreatePublishContext(streamPath, streamArguments);
                    _publishStreamContexts.Add(streamPath, publishStreamContext);

                    var subscribeStreamContexts = _subscribeStreamContexts.GetValueOrDefault(streamPath)?.ToList() ??
                        new List<IRtmpSubscribeStreamContext>();

                    foreach (var subscribeStreamContext in subscribeStreamContexts)
                        subscribeStreamContext.ResetTimestamps();

                    var startedSubscriberClientIds = HandlePublishContinuation(streamPath, publishStreamContext);
                    SendPublishingStartedMessages(streamContext, subscribeStreamContexts, startedSubscriberClientIds);

                    return (PublishingStreamResult.Succeeded, subscribeStreamContexts);
                }
            }
        }

        private (PublishingStreamResult Result, IList<IRtmpSubscribeStreamContext> SubscribeStreamContexts) StartDirectPublishing(
            IRtmpPublishStreamContext publishStreamContext)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    var streamPath = publishStreamContext.StreamPath;

                    if (_publishStreamContexts.ContainsKey(streamPath))
                        return (PublishingStreamResult.AlreadyExists, new List<IRtmpSubscribeStreamContext>());

                    _publishStreamContexts.Add(streamPath, publishStreamContext);

                    var subscribeStreamContexts = _subscribeStreamContexts.GetValueOrDefault(streamPath)?.ToList() ??
                        new List<IRtmpSubscribeStreamContext>();

                    foreach (var subscribeStreamContext in subscribeStreamContexts)
                        subscribeStreamContext.ResetTimestamps();

                    var startedSubscriberClientIds = HandlePublishContinuation(streamPath, publishStreamContext);
                    SendDirectPublishingStartedMessages(subscribeStreamContexts, startedSubscriberClientIds);

                    return (PublishingStreamResult.Succeeded, subscribeStreamContexts);
                }
            }
        }

        private List<uint> HandlePublishContinuation(string streamPath, IRtmpPublishStreamContext publishStreamContext)
        {
            if (!_publishStreamContinuationContexts.Remove(streamPath, out var continuationContext))
                return new List<uint>();

            publishStreamContext.SetTimestampOffset(continuationContext.Timestamp);

            using var _ = continuationContext;
            return continuationContext.SubscriberClientIds;
        }

        private (bool Result, IList<IRtmpSubscribeStreamContext> SubscribeStreamContexts) StopPublishing(
            IRtmpPublishStreamContext publishStreamContext, bool allowContinuation)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    var streamPath = publishStreamContext.StreamPath;

                    if (publishStreamContext.StreamContext != null)
                    {
                        if (publishStreamContext.StreamContext.PublishContext != publishStreamContext ||
                            !_publishStreamContexts.Remove(streamPath))
                        {
                            return (false, new List<IRtmpSubscribeStreamContext>());
                        }

                        publishStreamContext.StreamContext.RemovePublishContext();
                    }
                    else
                    {
                        if (!_publishStreamContexts.Remove(streamPath))
                        {
                            return (false, new List<IRtmpSubscribeStreamContext>());
                        }
                    }

                    var subscribeStreamContexts = _subscribeStreamContexts.GetValueOrDefault(streamPath)?.ToList() ??
                        new List<IRtmpSubscribeStreamContext>();

                    if (subscribeStreamContexts.Any())
                    {
                        if (allowContinuation && _config.PublishStreamContinuationTimeout > TimeSpan.Zero)
                        {
                            var timestamp = publishStreamContext.TimestampOffset + Math.Max(publishStreamContext.VideoTimestamp, publishStreamContext.AudioTimestamp);
                            CreatePublishStreamContinuationContext(streamPath, timestamp, subscribeStreamContexts);
                        }
                        else
                        {
                            SendPublishingEndedMessages(subscribeStreamContexts);
                        }
                    }

                    return (true, subscribeStreamContexts);
                }
            }

            void CreatePublishStreamContinuationContext(
                string streamPath, uint timestamp, IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts)
            {
                var continuationContext = new PublishStreamContinuationContext(streamPath, timestamp, subscribeStreamContexts);

                if (_publishStreamContinuationContexts.Remove(streamPath, out var existingContext))
                {
                    existingContext.Dispose();
                }

                _publishStreamContinuationContexts[streamPath] = continuationContext;

                continuationContext.SetExpirationCallback(_config.PublishStreamContinuationTimeout,
                    () => FinalizePublishStreamContinuationContext(continuationContext));
            }
        }

        private (SubscribingStreamResult Result, IRtmpPublishStreamContext? PublishStreamContext) StartSubscribing(
            IRtmpStreamContext streamContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    if (streamContext.PublishContext != null)
                    {
                        SendBadSubscribingConnectionMessage(streamContext, "Already publishing.");
                        return (SubscribingStreamResult.AlreadyPublishing, null);
                    }

                    if (streamContext.SubscribeContext != null)
                    {
                        SendBadSubscribingConnectionMessage(streamContext, "Already subscribing.");
                        return (SubscribingStreamResult.AlreadySubscribing, null);
                    }

                    _publishStreamContexts.TryGetValue(streamPath, out var publishStreamContext);

                    if (!_subscribeStreamContexts.TryGetValue(streamPath, out var subscribers))
                    {
                        subscribers = new List<IRtmpSubscribeStreamContext>();
                        _subscribeStreamContexts[streamPath] = subscribers;
                    }

                    var subscribeStreamContext = streamContext.CreateSubscribeContext(streamPath, streamArguments);
                    subscribers.Add(subscribeStreamContext);

                    SendSubscribingStartedMessages(subscribeStreamContext, publishStreamContext);
                    return (SubscribingStreamResult.Succeeded, publishStreamContext);
                }
            }
        }

        private bool StopSubscribing(IRtmpSubscribeStreamContext subscribeStreamContext)
        {
            lock (_subscribingSyncLock)
            {
                var streamPath = subscribeStreamContext.StreamPath;

                if (subscribeStreamContext.StreamContext.SubscribeContext != subscribeStreamContext ||
                    !_subscribeStreamContexts.TryGetValue(streamPath, out var subscribers) ||
                    !subscribers.Remove(subscribeStreamContext))
                {
                    return false;
                }

                subscribeStreamContext.StreamContext.RemoveSubscribeContext();

                if (subscribers.Count == 0)
                    _subscribeStreamContexts.Remove(streamPath);

                return true;
            }
        }

        public async ValueTask<(PublishingStreamResult Result, IList<IRtmpSubscribeStreamContext> SubscribeStreamContexts)> StartPublishingAsync(
            IRtmpStreamContext streamContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var result = StartPublishing(streamContext, streamPath, streamArguments);

            if (result.Result == PublishingStreamResult.Succeeded && streamContext.PublishContext != null)
            {
                await _eventDispatcher.RtmpStreamPublishedAsync(streamContext.PublishContext);
            }

            return result;
        }

        public async ValueTask<(PublishingStreamResult Result, IList<IRtmpSubscribeStreamContext> SubscribeStreamContexts)> StartDirectPublishingAsync(
            IRtmpPublishStreamContext publishStreamContext)
        {
            var result = StartDirectPublishing(publishStreamContext);

            if (result.Result == PublishingStreamResult.Succeeded)
            {
                await _eventDispatcher.RtmpStreamPublishedAsync(publishStreamContext);
            }

            return result;
        }

        public async ValueTask<(bool Result, IList<IRtmpSubscribeStreamContext> SubscribeStreamContexts)> StopPublishingAsync(
            IRtmpPublishStreamContext publishStreamContext, bool allowContinuation)
        {
            var result = StopPublishing(publishStreamContext, allowContinuation);

            if (result.Result)
            {
                await _eventDispatcher.RtmpStreamUnpublishedAsync(publishStreamContext, allowContinuation);
            }

            return result;
        }

        public async ValueTask<(SubscribingStreamResult Result, IRtmpPublishStreamContext? PublishStreamContext)> StartSubscribingAsync(
            IRtmpStreamContext streamContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var result = StartSubscribing(streamContext, streamPath, streamArguments);

            if (result.Result == SubscribingStreamResult.Succeeded && streamContext.SubscribeContext != null)
            {
                await _eventDispatcher.RtmpStreamSubscribedAsync(streamContext.SubscribeContext);
            }

            return result;
        }

        public async ValueTask<bool> StopSubscribingAsync(IRtmpSubscribeStreamContext subscribeStreamContext)
        {
            var result = StopSubscribing(subscribeStreamContext);

            if (result)
            {
                await _eventDispatcher.RtmpStreamUnsubscribedAsync(subscribeStreamContext);
            }

            return result;
        }

        public bool IsStreamPublishing(string streamPath)
        {
            lock (_publishingSyncLock)
            {
                return _publishStreamContexts.ContainsKey(streamPath);
            }
        }

        public IReadOnlyList<string> GetStreamPaths()
        {
            lock (_publishingSyncLock)
            {
                return _publishStreamContexts.Keys.ToList();
            }
        }

        public IRtmpPublishStreamContext? GetPublishStreamContext(string streamPath)
        {
            lock (_publishingSyncLock)
            {
                return _publishStreamContexts.GetValueOrDefault(streamPath);
            }
        }

        public IReadOnlyList<IRtmpSubscribeStreamContext> GetSubscribeStreamContexts(string streamPath)
        {
            lock (_subscribingSyncLock)
            {
                return _subscribeStreamContexts.GetValueOrDefault(streamPath)?.ToList() ??
                    new List<IRtmpSubscribeStreamContext>();
            }
        }

        public bool IsStreamBeingSubscribed(string streamPath)
        {
            lock (_subscribingSyncLock)
            {
                return _subscribeStreamContexts.ContainsKey(streamPath);
            }
        }

        private void FinalizePublishStreamContinuationContext(PublishStreamContinuationContext context)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    using var _ = context;

                    if (_publishStreamContinuationContexts.GetValueOrDefault(context.StreamPath) != context)
                    {
                        return;
                    }

                    var subscribeStreamContexts = _subscribeStreamContexts.GetValueOrDefault(context.StreamPath);

                    if (subscribeStreamContexts != null)
                    {
                        SendPublishingEndedMessages(subscribeStreamContexts);
                    }

                    _publishStreamContinuationContexts.Remove(context.StreamPath);
                }
            }
        }

        private void SendAlreadyExistsMessage(IRtmpStreamContext publisherStreamContext)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                publisherStreamContext,
                RtmpStatusLevels.Error,
                RtmpStreamStatusCodes.PublishBadName,
                "Stream already exists.");
        }

        private void SendBadPublishingConnectionMessage(IRtmpStreamContext publisherStreamContext, string reason)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                publisherStreamContext,
                RtmpStatusLevels.Error,
                RtmpStreamStatusCodes.PublishBadConnection,
                reason);
        }

        private void SendPublishingEndedMessages(IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts)
        {
            var subscriberStreamContexts = subscribeStreamContexts.Select(x => x.StreamContext).ToList();

            _commandMessageSender.SendOnStatusCommandMessage(
                subscriberStreamContexts,
                RtmpStatusLevels.Status,
                RtmpStreamStatusCodes.PlayUnpublishNotify,
                "Stream is unpublished.");

            _userControlMessageSender.SendStreamEofMessage(subscribeStreamContexts);
        }

        private void SendPublishingStartedMessages(
            IRtmpStreamContext publisherStreamContext,
            IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts,
            IReadOnlyList<uint> startedSubscriberClientIds)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                publisherStreamContext,
                RtmpStatusLevels.Status,
                RtmpStreamStatusCodes.PublishStart,
                "Publishing started.");

            SendDirectPublishingStartedMessages(subscribeStreamContexts, startedSubscriberClientIds);
        }

        private void SendDirectPublishingStartedMessages(
            IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts, IReadOnlyList<uint> startedSubscriberClientIds)
        {
            var newSubscribeStreamContexts = subscribeStreamContexts
                .ExceptBy(startedSubscriberClientIds, x => x.StreamContext.ClientContext.Client.Id)
                .ToList();

            SendStreamStartedMessages(subscribeStreamContexts, newSubscribeStreamContexts);
        }

        private void SendStreamStartedMessages(
            IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts,
            List<IRtmpSubscribeStreamContext> subscriberStream)
        {
            _userControlMessageSender.SendStreamBeginMessage(subscriberStream);

            var streamContexts = subscribeStreamContexts
                .Select(x => x.StreamContext).ToList();

            _commandMessageSender.SendOnStatusCommandMessage(
               streamContexts,
               RtmpStatusLevels.Status,
               RtmpStreamStatusCodes.PlayReset,
               "Stream started.");

            _commandMessageSender.SendOnStatusCommandMessage(
                streamContexts,
                RtmpStatusLevels.Status,
                RtmpStreamStatusCodes.PlayStart,
                "Stream started.");
        }

        private void SendSubscribingStartedMessages(
            IRtmpSubscribeStreamContext subscribeStreamContext, IRtmpPublishStreamContext? publishStreamContext)
        {
            if (publishStreamContext == null)
                return;

            _userControlMessageSender.SendStreamBeginMessage(subscribeStreamContext);

            _commandMessageSender.SendOnStatusCommandMessage(
                subscribeStreamContext.StreamContext,
                RtmpStatusLevels.Status,
                RtmpStreamStatusCodes.PlayReset,
                "Stream subscribed.");

            _commandMessageSender.SendOnStatusCommandMessage(
                subscribeStreamContext.StreamContext,
                RtmpStatusLevels.Status,
                RtmpStreamStatusCodes.PlayStart,
                "Stream subscribed.");
        }

        private void SendBadSubscribingConnectionMessage(IRtmpStreamContext subscriberStreamContext, string reason)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                subscriberStreamContext,
                RtmpStatusLevels.Error,
                RtmpStreamStatusCodes.PlayBadConnection,
                reason);
        }

        private class PublishStreamContinuationContext : IDisposable
        {
            public string StreamPath { get; }
            public uint Timestamp { get; }
            public List<uint> SubscriberClientIds { get; }

            private readonly CancellationTokenSource _cts = new();
            private int _isDisposed;

            public PublishStreamContinuationContext(
                string streamPath,
                uint timestamp,
                IReadOnlyList<IRtmpSubscribeStreamContext> subscribeStreamContexts)
            {
                StreamPath = streamPath;
                Timestamp = timestamp;
                SubscriberClientIds = subscribeStreamContexts.Select(x => x.StreamContext.ClientContext.Client.Id).ToList();
            }

            public void SetExpirationCallback(TimeSpan expiration, Action callback)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(expiration, _cts.Token);
                        callback.Invoke();
                    }
                    catch (OperationCanceledException) when (_cts.IsCancellationRequested) { }
                });
            }

            public void Dispose()
            {
                if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
                    return;

                _cts.Cancel();
            }
        }
    }
}
