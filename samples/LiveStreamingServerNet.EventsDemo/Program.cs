using LiveStreamingServerNet.Networking;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net;

namespace LiveStreamingServerNet.EventsDemo
{
    public static class Program
    {
        public static async Task Main()
        {
            using var liveStreamingServer = LiveStreamingServerBuilder.Create()
                .ConfigureRtmpServer(options =>
                {
                    options.Services.Configure<IdlePublishingTimeLimiterConfig>(config =>
                        config.IdlePublishingTimeLimitSeconds = 60
                    );

                    options.AddStreamEventHandler<IdlePublishingTimeLimiter>();
                })
                .ConfigureLogging(options => options.AddConsole())
                .Build();

            await liveStreamingServer.RunAsync(new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false));
        }
    }

    public class IdlePublishingTimeLimiterConfig
    {
        public int IdlePublishingTimeLimitSeconds { get; set; }
    }

    public class IdlePublishingTimeLimiter : IRtmpServerStreamEventHandler, IDisposable
    {
        private readonly ConcurrentDictionary<uint, ITimer> _clientTimers = new();
        private readonly IRtmpStreamInfoManager _streamInfoManager;
        private readonly IdlePublishingTimeLimiterConfig _config;

        public IdlePublishingTimeLimiter(IRtmpStreamInfoManager streamInfoManager, IOptions<IdlePublishingTimeLimiterConfig> config)
        {
            _streamInfoManager = streamInfoManager;
            _config = config.Value;
        }

        public void Dispose()
        {
            foreach (var timer in _clientTimers.Values)
                timer.Dispose();

            _clientTimers.Clear();
        }

        public ValueTask OnRtmpStreamPublishedAsync(
            IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            _clientTimers[clientId] = new Timer(async _ =>
            {
                var stream = _streamInfoManager.GetStreamInfo(streamPath);

                if (stream != null && stream.Publisher.Id == clientId && stream.Subscribers.Count == 0)
                    await stream.Publisher.DisconnectAsync();

            }, null, TimeSpan.FromSeconds(_config.IdlePublishingTimeLimitSeconds), Timeout.InfiniteTimeSpan);

            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpStreamUnpublishedAsync(IEventContext context, uint clientId, string streamPath)
        {
            if (_clientTimers.TryRemove(clientId, out var timer))
                timer.Dispose();

            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpStreamMetaDataReceivedAsync(
            IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpStreamSubscribedAsync(
            IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpStreamUnsubscribedAsync(IEventContext context, uint clientId, string streamPath)
            => ValueTask.CompletedTask;
    }
}
