using LiveStreamingServerNet.Flv.Internal.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services.Contracts
{
    internal interface IFlvStreamManagerService
    {
        bool IsStreamPathPublishing(string publishStreamPath);

        PublishingStreamResult StartPublishingStream(string streamPath, IDictionary<string, string> streamArguments);
        bool StopPublishingStream(string streamPath, out IList<IFlvClient> existingSubscribers);

        SubscribingStreamResult StartSubscribingStream(IFlvClient client, string streamPath);
        bool StopSubscribingStream(IFlvClient client);
    }

    internal enum PublishingStreamResult
    {
        Succeeded,
        AlreadyExists,
        AlreadyPublishing
    }

    internal enum SubscribingStreamResult
    {
        Succeeded,
        StreamDoesntExist,
        AlreadySubscribing,
    }
}
