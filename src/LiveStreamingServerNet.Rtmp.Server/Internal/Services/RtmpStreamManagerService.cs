using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Extensions;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Services
{
    internal class RtmpStreamManagerService : IRtmpStreamManagerService
    {
        private readonly object _publishingSyncLock = new();
        private readonly Dictionary<string, IRtmpPublishStreamContext> _publishStreamContexts = new();

        private readonly object _subscribingSyncLock = new();
        private readonly Dictionary<string, List<IRtmpSubscribeStreamContext>> _subscribeStreamContexts = new();

        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpUserControlMessageSenderService _userControlMessageSender;

        public RtmpStreamManagerService(
            IRtmpCommandMessageSenderService commandMessageSender,
            IRtmpUserControlMessageSenderService userControlMessageSender)
        {
            _commandMessageSender = commandMessageSender;
            _userControlMessageSender = userControlMessageSender;
        }

        public PublishingStreamResult StartPublishing(
            IRtmpStreamContext streamContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments, out IList<IRtmpSubscribeStreamContext> subscribeStreamContexts)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    subscribeStreamContexts = null!;

                    if (streamContext.PublishContext != null)
                    {
                        SendBadPublishingConnectionMessage(streamContext, "Already publishing.");
                        return PublishingStreamResult.AlreadyPublishing;
                    }

                    if (streamContext.SubscribeContext != null)
                    {
                        SendBadPublishingConnectionMessage(streamContext, "Already subscribing.");
                        return PublishingStreamResult.AlreadySubscribing;
                    }

                    if (_publishStreamContexts.ContainsKey(streamPath))
                    {
                        SendAlreadyExistsMessage(streamContext);
                        return PublishingStreamResult.AlreadyExists;
                    }

                    var publishStreamContext = streamContext.CreatePublishContext(streamPath, streamArguments);
                    _publishStreamContexts.Add(streamPath, publishStreamContext);

                    subscribeStreamContexts = _subscribeStreamContexts.GetValueOrDefault(streamPath)?.ToList() ??
                        new List<IRtmpSubscribeStreamContext>();

                    foreach (var subscribeStreamContext in subscribeStreamContexts)
                        subscribeStreamContext.ResetTimestamps();

                    SendPublishingStartedMessages(streamContext, subscribeStreamContexts);
                    return PublishingStreamResult.Succeeded;
                }
            }
        }

        public PublishingStreamResult StartDirectPublishing(
            IRtmpPublishStreamContext publishStreamContext, out IList<IRtmpSubscribeStreamContext> subscribeStreamContexts)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    subscribeStreamContexts = null!;

                    var streamPath = publishStreamContext.StreamPath;

                    if (_publishStreamContexts.ContainsKey(streamPath))
                        return PublishingStreamResult.AlreadyExists;

                    _publishStreamContexts.Add(streamPath, publishStreamContext);

                    subscribeStreamContexts = _subscribeStreamContexts.GetValueOrDefault(streamPath)?.ToList() ??
                        new List<IRtmpSubscribeStreamContext>();

                    foreach (var subscribeStreamContext in subscribeStreamContexts)
                        subscribeStreamContext.ResetTimestamps();

                    SendDirectPublishingStartedMessages(subscribeStreamContexts);
                    return PublishingStreamResult.Succeeded;
                }
            }
        }

        public bool StopPublishing(IRtmpPublishStreamContext publishStreamContext, out IList<IRtmpSubscribeStreamContext> subscribeStreamContexts)
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
                            subscribeStreamContexts = new List<IRtmpSubscribeStreamContext>();
                            return false;
                        }

                        publishStreamContext.StreamContext.RemovePublishContext();
                    }
                    else
                    {
                        if (!_publishStreamContexts.Remove(streamPath))
                        {
                            subscribeStreamContexts = new List<IRtmpSubscribeStreamContext>();
                            return false;
                        }
                    }

                    subscribeStreamContexts = _subscribeStreamContexts.GetValueOrDefault(streamPath)?.ToList() ??
                        new List<IRtmpSubscribeStreamContext>();

                    SendPublishingEndedMessages(subscribeStreamContexts.AsReadOnly());
                    return true;
                }
            }
        }

        public bool IsStreamPublishing(string streamPath)
        {
            lock (_publishingSyncLock)
            {
                return _publishStreamContexts.ContainsKey(streamPath);
            }
        }

        public SubscribingStreamResult StartSubscribing(
            IRtmpStreamContext streamContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments, out IRtmpPublishStreamContext? publishStreamContext)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    publishStreamContext = null;

                    if (streamContext.PublishContext != null)
                    {
                        SendBadSubscribingConnectionMessage(streamContext, "Already publishing.");
                        return SubscribingStreamResult.AlreadyPublishing;
                    }

                    if (streamContext.SubscribeContext != null)
                    {
                        SendBadSubscribingConnectionMessage(streamContext, "Already subscribing.");
                        return SubscribingStreamResult.AlreadySubscribing;
                    }

                    _publishStreamContexts.TryGetValue(streamPath, out publishStreamContext);

                    if (!_subscribeStreamContexts.TryGetValue(streamPath, out var subscribers))
                    {
                        subscribers = new List<IRtmpSubscribeStreamContext>();
                        _subscribeStreamContexts[streamPath] = subscribers;
                    }

                    var subscribeStreamContext = streamContext.CreateSubscribeContext(streamPath, streamArguments);
                    subscribers.Add(subscribeStreamContext);

                    SendSubscribingStartedMessages(subscribeStreamContext, publishStreamContext);
                    return SubscribingStreamResult.Succeeded;
                }
            }
        }

        public bool StopSubscribing(IRtmpSubscribeStreamContext subscribeStreamContext)
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

        private void SendPublishingEndedMessages(IList<IRtmpSubscribeStreamContext> subscribeStreamContexts)
        {
            var subscriberStreamContexts = subscribeStreamContexts.Select(x => x.StreamContext).ToList();

            _commandMessageSender.SendOnStatusCommandMessage(
                subscriberStreamContexts,
                RtmpStatusLevels.Status,
                RtmpStreamStatusCodes.PlayUnpublishNotify,
                "Stream is unpublished.");

            _userControlMessageSender.SendStreamEofMessage(subscribeStreamContexts.AsReadOnly());
        }

        private void SendPublishingStartedMessages(
            IRtmpStreamContext publisherStreamContext, IList<IRtmpSubscribeStreamContext> subscribeStreamContexts)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                publisherStreamContext,
                RtmpStatusLevels.Status,
                RtmpStreamStatusCodes.PublishStart,
                "Publishing started.");

            SendDirectPublishingStartedMessages(subscribeStreamContexts);
        }

        private void SendDirectPublishingStartedMessages(IList<IRtmpSubscribeStreamContext> subscribeStreamContexts)
        {
            _userControlMessageSender.SendStreamBeginMessage(subscribeStreamContexts.AsReadOnly());

            var subscriberStreamContexts = subscribeStreamContexts.Select(x => x.StreamContext).ToList();

            _commandMessageSender.SendOnStatusCommandMessage(
               subscriberStreamContexts,
               RtmpStatusLevels.Status,
               RtmpStreamStatusCodes.PlayReset,
               "Stream started.");

            _commandMessageSender.SendOnStatusCommandMessage(
                subscriberStreamContexts,
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
    }
}
