using LiveStreamingServerNet.Standalone.Internal.Contracts;

namespace LiveStreamingServerNet.Standalone.Internal.Services.Contracts
{
    internal interface IRtmpStreamManagerService
    {
        Task RtmpStreamPublishedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        Task RtmpStreamUnpublishedAsync(uint clientId, string streamPath);
        Task RtmpStreamSubscribedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        Task RtmpStreamUnsubscribedAsync(uint clientId, string streamPath);
        Task RtmpStreamMetaDataReceived(uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData);

        IRtmpPublishStream? GetStream(string id);
        IList<IRtmpPublishStream> GetStreams();
    }
}
