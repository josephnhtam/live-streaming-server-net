using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Auth.Contracts
{
    public interface IAuthorizationHandler
    {
        Task<AuthorizationResult> AuthorizePublishingAsync(IClientInfo client, string streamPath, IDictionary<string, string> streamArguments, string publishingType);
        Task<AuthorizationResult> AuthorizeSubscriptionAsync(IClientInfo client, string streamPath, IDictionary<string, string> streamArguments);
    }
}
