using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Contracts
{
    public interface IRtmpServerConnectionEventHandler
    {
        ValueTask OnRtmpClientCreatedAsync(IClientControl client);
        ValueTask OnRtmpClientDisposedAsync(uint clientId);

        ValueTask OnRtmpClientHandshakeCompleteAsync(uint clientId);
        ValueTask OnRtmpClientConnectedAsync(uint clientId, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments);
    }

    public interface IRtmpServerStreamEventHandler
    {
        ValueTask OnRtmpStreamPublishedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        ValueTask OnRtmpStreamUnpublishedAsync(uint clientId, string streamPath);
        ValueTask OnRtmpStreamSubscribedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        ValueTask OnRtmpStreamUnsubscribedAsync(uint clientId, string streamPath);

        ValueTask OnRtmpStreamMetaDataReceivedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData);
    }
}
