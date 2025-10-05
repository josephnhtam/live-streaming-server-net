using LiveStreamingServerNet.Flv.Internal.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services.Contracts
{
    internal interface IFlvStreamManagerService
    {
        IList<string> GetStreamPaths();
        bool IsStreamPathPublishing(string streamPath, bool requireReady = true);

        PublishingStreamResult StartPublishingStream(IFlvStreamContext streamContext);
        bool StopPublishingStream(string streamPath, bool allowContinuation, out IList<IFlvClient> existingSubscribers);
        IFlvStreamContext? GetFlvStreamContext(string streamPath);

        ValueTask<SubscribingStreamResult> StartSubscribingStreamAsync(IFlvClient client, string streamPath, bool requireReady = true);
        ValueTask<bool> StopSubscribingStreamAsync(IFlvClient client);
        IReadOnlyList<IFlvClient> GetSubscribers(string streamPath);
    }

    internal enum PublishingStreamResult
    {
        Succeeded,
        AlreadyExists
    }

    internal enum SubscribingStreamResult
    {
        Succeeded,
        StreamDoesntExist,
        AlreadySubscribing,
    }
}
