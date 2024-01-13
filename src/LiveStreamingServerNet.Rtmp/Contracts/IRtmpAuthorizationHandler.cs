using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Contracts
{
    public interface IRtmpAuthorizationHandler
    {
        Task<bool> AuthorizePublishingAsync(IClientInfo client, string streamPath, IDictionary<string, string> streamArguments, string publishingType);
        Task<bool> AuthorizeSubscriptionAsync(IClientInfo client, string streamPath, IDictionary<string, string> streamArguments);
    }
}
