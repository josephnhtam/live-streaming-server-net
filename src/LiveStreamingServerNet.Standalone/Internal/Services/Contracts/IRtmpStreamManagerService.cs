using LiveStreamingServerNet.Standalone.Internal.Contracts;

namespace LiveStreamingServerNet.Standalone.Internal.Services.Contracts
{
    internal interface IRtmpStreamManagerService
    {
        ValueTask RtmpStreamPublishedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        ValueTask RtmpStreamUnpublishedAsync(uint clientId, string streamPath);
        ValueTask RtmpStreamSubscribedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
        ValueTask RtmpStreamUnsubscribedAsync(uint clientId, string streamPath);
        ValueTask RtmpStreamMetaDataReceivedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData);

        IRtmpPublishStream? GetStream(string id);
        IList<IRtmpPublishStream> GetStreams();
    }
}
