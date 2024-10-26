using LiveStreamingServerNet.Flv.Configurations;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class FlvStreamManagerService : IFlvStreamManagerService
    {
        private readonly object _publishingSyncLock = new();
        private readonly Dictionary<string, IFlvStreamContext> _publishingStreamContexts = new();
        private readonly Dictionary<string, FlvStreamContinuationContext> _streamContinuationContexts = new();

        private readonly object _subscribingSyncLock = new();
        private readonly Dictionary<string, List<IFlvClient>> _subscribingClients = new();
        private readonly Dictionary<IFlvClient, string> _subscribedStreamPaths = new();

        private readonly FlvConfiguration _config;

        public FlvStreamManagerService(IOptions<FlvConfiguration> config)
        {
            _config = config.Value;
        }

        public bool IsStreamPathPublishing(string streamPath, bool requireReady)
        {
            lock (_publishingSyncLock)
            {
                return _publishingStreamContexts.TryGetValue(streamPath, out var streamContext) && (!requireReady || streamContext.IsReady);
            }
        }

        public PublishingStreamResult StartPublishingStream(IFlvStreamContext streamContext)
        {
            lock (_publishingSyncLock)
            {
                var streamPath = streamContext.StreamPath;

                if (_publishingStreamContexts.ContainsKey(streamPath))
                {
                    streamContext.Dispose();
                    return PublishingStreamResult.AlreadyExists;
                }

                HandleStreamContinuation(streamPath, streamContext);
                _publishingStreamContexts.Add(streamPath, streamContext);
                return PublishingStreamResult.Succeeded;
            }

            void HandleStreamContinuation(string streamPath, IFlvStreamContext streamContext)
            {
                if (!_streamContinuationContexts.Remove(streamPath, out var continuationContext))
                {
                    return;
                }

                streamContext.SetTimestampOffset(continuationContext.Timestamp);
                continuationContext.Dispose();
            }
        }

        public bool StopPublishingStream(string streamPath, bool allowContinuation, out IList<IFlvClient> existingSubscribers)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    if (!_publishingStreamContexts.Remove(streamPath, out var streamContext))
                    {
                        existingSubscribers = new List<IFlvClient>();
                        return false;
                    }

                    existingSubscribers = _subscribingClients.GetValueOrDefault(streamPath)?.ToList() ?? new List<IFlvClient>();

                    if (existingSubscribers.Any())
                    {
                        if (allowContinuation && _config.StreamContinuationTimeout > TimeSpan.Zero)
                        {
                            var timestamp = streamContext.TimestampOffset + Math.Max(streamContext.VideoTimestamp, streamContext.AudioTimestamp);
                            CreateStreamContinuationContext(streamPath, timestamp, existingSubscribers.AsReadOnly());
                        }
                        else
                        {
                            StopSubscribers(existingSubscribers.AsReadOnly());
                        }
                    }

                    streamContext.Dispose();

                    return true;
                }
            }

            void CreateStreamContinuationContext(string streamPath, uint timestamp, IReadOnlyList<IFlvClient> subscribers)
            {
                var continuationContext = new FlvStreamContinuationContext(streamPath, timestamp, subscribers);

                if (_streamContinuationContexts.Remove(streamPath, out var existingContext))
                {
                    existingContext.Dispose();
                }

                _streamContinuationContexts[streamPath] = continuationContext;

                continuationContext.SetExpirationCallback(
                     _config.StreamContinuationTimeout,
                    () => FinalizeStreamContinuationContext(continuationContext));
            }
        }

        private void FinalizeStreamContinuationContext(FlvStreamContinuationContext context)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    using var _ = context;

                    if (_streamContinuationContexts.GetValueOrDefault(context.StreamPath) != context)
                    {
                        return;
                    }

                    var subscribers = _subscribingClients.GetValueOrDefault(context.StreamPath);

                    if (subscribers?.Any() == true)
                    {
                        StopSubscribers(subscribers);
                    }

                    _streamContinuationContexts.Remove(context.StreamPath);
                }
            }
        }

        private void StopSubscribers(IReadOnlyList<IFlvClient> subscribers)
        {
            var subscriberList = subscribers.ToList();

            foreach (var subscriber in subscriberList)
            {
                subscriber.Stop();
            }
        }

        public IFlvStreamContext? GetFlvStreamContext(string streamPath)
        {
            lock (_publishingSyncLock)
            {
                return _publishingStreamContexts.GetValueOrDefault(streamPath);
            }
        }

        public SubscribingStreamResult StartSubscribingStream(IFlvClient client, string streamPath, bool requireReady)
        {
            lock (_publishingSyncLock)
            {
                lock (_subscribingSyncLock)
                {
                    if (_subscribedStreamPaths.ContainsKey(client))
                        return SubscribingStreamResult.AlreadySubscribing;

                    if (!_publishingStreamContexts.TryGetValue(streamPath, out var stream) || (requireReady && !stream.IsReady))
                        return SubscribingStreamResult.StreamDoesntExist;

                    if (!_subscribingClients.TryGetValue(streamPath, out var subscribers))
                    {
                        subscribers = new List<IFlvClient>();
                        _subscribingClients[streamPath] = subscribers;
                    }

                    subscribers.Add(client);
                    _subscribedStreamPaths[client] = streamPath;

                    return SubscribingStreamResult.Succeeded;
                }
            }
        }

        public bool StopSubscribingStream(IFlvClient client)
        {
            lock (_subscribingSyncLock)
            {
                if (!_subscribedStreamPaths.Remove(client, out var streamPath))
                    return false;

                if (_subscribingClients.TryGetValue(streamPath, out var subscribers) &&
                    subscribers.Remove(client) && subscribers.Count == 0)
                    _subscribingClients.Remove(streamPath);

                return true;
            }
        }

        public IReadOnlyList<IFlvClient> GetSubscribers(string streamPath)
        {
            lock (_subscribingSyncLock)
            {
                return _subscribingClients.GetValueOrDefault(streamPath)?.ToList() ?? new List<IFlvClient>();
            }
        }

        private class FlvStreamContinuationContext : IDisposable
        {
            public string StreamPath { get; }
            public uint Timestamp { get; }
            public List<string> SubscriberClientIds { get; }

            private readonly CancellationTokenSource _cts = new();
            private int _isDisposed;

            public FlvStreamContinuationContext(
                string streamPath,
                uint timestamp,
                IReadOnlyList<IFlvClient> subscribers)
            {
                StreamPath = streamPath;
                Timestamp = timestamp;
                SubscriberClientIds = subscribers.Select(x => x.ClientId).ToList();
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
