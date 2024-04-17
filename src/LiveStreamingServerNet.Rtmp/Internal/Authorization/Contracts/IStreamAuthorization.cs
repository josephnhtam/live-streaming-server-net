using LiveStreamingServerNet.Rtmp.Auth;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal.Authorization.Contracts
{
    internal interface IStreamAuthorization
    {
        ValueTask<AuthorizationResult> AuthorizePublishingAsync(
            IRtmpClientContext clientContext,
            string streamPath,
            string publishingType,
            IReadOnlyDictionary<string, string> streamArguments);

        ValueTask<AuthorizationResult> AuthorizeSubscribingAsync(
           IRtmpClientContext clientContext,
           string streamPath,
           IReadOnlyDictionary<string, string> streamArguments);
    }
}
