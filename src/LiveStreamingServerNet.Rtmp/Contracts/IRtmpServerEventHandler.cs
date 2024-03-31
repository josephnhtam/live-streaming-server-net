using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Rtmp.Contracts
{
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
}
