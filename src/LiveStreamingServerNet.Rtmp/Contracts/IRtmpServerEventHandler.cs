using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Contracts
{
    public interface IRtmpServerConnectionEventHandler
    {
        Task OnRtmpClientCreatedAsync(IClientControl client);
        Task OnRtmpClientDisposedAsync(uint clientId);

        Task OnRtmpClientHandshakeCompleteAsync(uint clientId);
        Task OnRtmpClientConnectedAsync(uint clientId, IReadOnlyDictionary<string, object> commandObject, IReadOnlyDictionary<string, object>? arguments);
    }

    public interface IRtmpServerStreamEventHandler
    {
        Task OnRtmpStreamPublishedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        Task OnRtmpStreamUnpublishedAsync(uint clientId, string streamPath);
        Task OnRtmpStreamSubscribedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        Task OnRtmpStreamUnsubscribedAsync(uint clientId, string streamPath);

        Task OnRtmpStreamMetaDataReceived(uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData);
    }
}
