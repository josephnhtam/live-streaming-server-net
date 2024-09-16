using LiveStreamingServerNet.Rtmp.Server.Auth;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Authorization.Contracts
{
    internal interface IStreamAuthorization
    {
        ValueTask<AuthorizationResult> AuthorizePublishingAsync(
            IRtmpClientSessionContext clientContext,
            string streamPath,
            string publishingType,
            IReadOnlyDictionary<string, string> streamArguments);

        ValueTask<AuthorizationResult> AuthorizeSubscribingAsync(
            IRtmpClientSessionContext clientContext,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments);
    }
}