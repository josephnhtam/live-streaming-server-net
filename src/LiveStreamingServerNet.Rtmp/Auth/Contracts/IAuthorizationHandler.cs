using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Auth.Contracts
{
    public interface IAuthorizationHandler
    {
        int GetOrder() => 0;
        Task<AuthorizationResult> AuthorizePublishingAsync(IClientInfo client, string streamPath, IReadOnlyDictionary<string, string> streamArguments, string publishingType);
        Task<AuthorizationResult> AuthorizeSubscribingAsync(IClientInfo client, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
