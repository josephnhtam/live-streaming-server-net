using LiveStreamingServerNet.Rtmp.Auth;
using LiveStreamingServerNet.Rtmp.Auth.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Authorization.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Rtmp.Internal.Authorization
{
    internal class StreamAuthorization : IStreamAuthorization
    {
        private readonly IServiceProvider _services;
        private readonly IRtmpServerContext _serverContext;

        public StreamAuthorization(
           IServiceProvider services,
           IRtmpServerContext serverContext)
        {
            _services = services;
            _serverContext = serverContext;
        }

        public async ValueTask<AuthorizationResult> AuthorizePublishingAsync(IRtmpClientContext clientContext, string streamPath, string publishingType, IReadOnlyDictionary<string, string> streamArguments)
        {
            if (IsAuthorizedByCode(streamArguments))
                return AuthorizationResult.Authorized();

            foreach (var authorizationHandler in _services.GetServices<IAuthorizationHandler>().OrderBy(x => x.GetOrder()))
            {
                var result = await authorizationHandler.AuthorizePublishingAsync(
                    clientContext.Client, streamPath, streamArguments, publishingType);

                if (!result.IsAuthorized)
                    return result;

                streamPath = result.StreamPathOverride ?? streamPath;
                streamArguments = result.StreamArgumentsOverride ?? streamArguments;
            }

            return AuthorizationResult.Authorized(streamPath, streamArguments);
        }

        public async ValueTask<AuthorizationResult> AuthorizeSubscribingAsync(IRtmpClientContext clientContext, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            if (IsAuthorizedByCode(streamArguments))
                return AuthorizationResult.Authorized();

            foreach (var authorizationHandler in _services.GetServices<IAuthorizationHandler>().OrderBy(x => x.GetOrder()))
            {
                var result = await authorizationHandler.AuthorizeSubscribingAsync(
                    clientContext.Client, streamPath, streamArguments);

                if (!result.IsAuthorized)
                    return result;

                streamPath = result.StreamPathOverride ?? streamPath;
                streamArguments = result.StreamArgumentsOverride ?? streamArguments;
            }

            return AuthorizationResult.Authorized(streamPath, streamArguments);
        }

        private bool IsAuthorizedByCode(IReadOnlyDictionary<string, string> streamArguments)
        {
            return streamArguments.TryGetValue("code", out var authCode) && authCode == _serverContext.AuthCode;
        }
    }
}
