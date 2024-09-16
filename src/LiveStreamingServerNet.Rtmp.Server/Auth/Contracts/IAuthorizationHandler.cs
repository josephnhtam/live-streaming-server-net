using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Auth.Contracts
{
    public interface IAuthorizationHandler
    {
        int GetOrder() => 0;
        Task<AuthorizationResult> AuthorizePublishingAsync(ISessionInfo client, string streamPath, IReadOnlyDictionary<string, string> streamArguments, string publishingType);
        Task<AuthorizationResult> AuthorizeSubscribingAsync(ISessionInfo client, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
