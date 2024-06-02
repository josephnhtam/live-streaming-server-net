# RTMP Server Events

There are multiple event handlers available, including `IRtmpServerConnectionEventHandler`, `IRtmpServerStreamEventHandler` and `IRtmpMediaMessageInterceptor`, which are useful for extending the behavior of LiveStreamingServerNet.

## Interfaces

Below are the interfaces for these event handlers:

```cs
public interface IRtmpServerConnectionEventHandler
{
    int GetOrder() => 0;
    ValueTask OnRtmpClientCreatedAsync(IEventContext context, IClientControl client);
    ValueTask OnRtmpClientDisposedAsync(IEventContext context, uint clientId);
    ValueTask OnRtmpClientHandshakeCompleteAsync(IEventContext context, uint clientId);
    ValueTask OnRtmpClientConnectedAsync(IEventContext context, uint clientId, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments);
}
public interface IRtmpServerStreamEventHandler
{
    int GetOrder() => 0;
    ValueTask OnRtmpStreamPublishedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    ValueTask OnRtmpStreamUnpublishedAsync(IEventContext context, uint clientId, string streamPath);
    ValueTask OnRtmpStreamSubscribedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    ValueTask OnRtmpStreamUnsubscribedAsync(IEventContext context, uint clientId, string streamPath);
    ValueTask OnRtmpStreamMetaDataReceivedAsync(IEventContext context, uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData);
}

public interface IRtmpMediaMessageInterceptor
{
    ValueTask OnCacheSequenceHeader(string streamPath, MediaType mediaType, byte[] sequenceHeader);
    ValueTask OnCachePicture(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp);
    ValueTask OnClearGroupOfPicturesCache(string streamPath);
    ValueTask OnReceiveMediaMessage(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable);
}
```

## Usage Example

For instance, if you want to limit the publishing time of every stream to a maximum of 30 minutes, you can do the following:

### Implement IRtmpServerStreamEventHandler

```cs
public class PublishingTimeLimiterConfig
{
    public int PublishingTimeLimitSeconds { get; set; }
}

public class PublishingTimeLimiter : IRtmpServerStreamEventHandler, IDisposable
{
    private readonly ConcurrentDictionary<uint, ITimer> _clientTimers = new();
    private readonly IServer _server;
    private readonly PublishingTimeLimiterConfig _config;

    public PublishingTimeLimiter(IServer server, IOptions<PublishingTimeLimiterConfig> config)
    {
        _server = server;
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
            var client = _server.GetClient(clientId);

            if (client != null)
                await client.DisconnectAsync();
        }, null, TimeSpan.FromSeconds(_config.PublishingTimeLimitSeconds), Timeout.InfiniteTimeSpan);

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
```

The PublishingTimeLimiter, which implements `IRtmpServerStreamEventHandler`, will create a timer to disconnect the client after `PublishingTimeLimitSeconds`, and dispose the corresponding timer when the stream is unpublished.

### Register the Event Handler

```cs
using var liveStreamingServer = LiveStreamingServerBuilder.Create()
    .ConfigureRtmpServer(options =>
    {
        options.Services.Configure<PublishingTimeLimiterConfig>(config =>
            config.PublishingTimeLimitSeconds = 60 * 30
        );

        options.AddStreamEventHandler<PublishingTimeLimiter>();
    })
    .ConfigureLogging(options => options.AddConsole())
    .Build();

await liveStreamingServer.RunAsync(new ServerEndPoint(new IPEndPoint(IPAddress.Any, 1935), false));
```

This code adds the implementation of `IRtmpServerStreamEventHandler` to the RTMP server.
