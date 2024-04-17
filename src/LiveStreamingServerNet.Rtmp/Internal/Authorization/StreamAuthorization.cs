using LiveStreamingServerNet.Rtmp.Auth;
using LiveStreamingServerNet.Rtmp.Auth.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Authorization.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (streamArguments.TryGetValue("code", out var authCode) && authCode == _serverContext.AuthCode)
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
            if (streamArguments.TryGetValue("code", out var authCode) && authCode == _serverContext.AuthCode)
                return AuthorizationResult.Authorized();

            foreach (var authorizationHandler in _services.GetServices<IAuthorizationHandler>().OrderBy(x => x.GetOrder()))
            {
                var result = await authorizationHandler.AuthorizeSubscriptionAsync(
                    clientContext.Client, streamPath, streamArguments);

                if (!result.IsAuthorized)
                    return result;

                streamPath = result.StreamPathOverride ?? streamPath;
                streamArguments = result.StreamArgumentsOverride ?? streamArguments;
            }

            return AuthorizationResult.Authorized(streamPath, streamArguments);
        }
    }
}
