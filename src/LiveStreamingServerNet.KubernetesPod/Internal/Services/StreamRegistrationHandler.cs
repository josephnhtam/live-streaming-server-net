using LiveStreamingServerNet.KubernetesPod.Internal.Services.Contracts;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Auth;
using LiveStreamingServerNet.Rtmp.Auth.Contracts;

namespace LiveStreamingServerNet.KubernetesPod.Internal.Services
{
    internal class StreamRegistrationHandler : IAuthorizationHandler
    {
        private readonly IStreamRegistry _streamRegistry;

        public const int Order = 100;
        int IAuthorizationHandler.GetOrder() => Order;

        public StreamRegistrationHandler(IStreamRegistry streamRegistry)
        {
            _streamRegistry = streamRegistry;
        }

        public async Task<AuthorizationResult> AuthorizePublishingAsync(
            IClientInfo client,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            string publishingType)
        {
            var result = await _streamRegistry.RegisterStreamAsync(client, streamPath, streamArguments);

            if (result.Successful)
                return AuthorizationResult.Authorized();

            return AuthorizationResult.Unauthorized(result.Reason ?? "Failed to register stream.");
        }

        public async Task<AuthorizationResult> AuthorizeSubscribingAsync(
            IClientInfo client,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            if (await _streamRegistry.IsStreamRegisteredAsync(streamPath, true))
                return AuthorizationResult.Authorized();

            return AuthorizationResult.Unauthorized("Stream not registered.");
        }
    }
}
