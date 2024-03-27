using LiveStreamingServerNet.KubernetesPod.Configurations;
using LiveStreamingServerNet.KubernetesPod.Internal.Logging;
using LiveStreamingServerNet.KubernetesPod.Services.Contracts;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Auth;
using LiveStreamingServerNet.Rtmp.Auth.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.KubernetesPod.Internal.Services
{
    internal class PodAuthorizationHandler : IAuthorizationHandler
    {
        private readonly IPodStatus _podStatus;
        private readonly KubernetesPodConfiguration _config;
        private readonly ILogger _logger;

        public PodAuthorizationHandler(
            IPodStatus podStatus,
            IOptions<KubernetesPodConfiguration> config,
            ILogger<PodAuthorizationHandler> logger)
        {
            _podStatus = podStatus;
            _config = config.Value;
            _logger = logger;
        }

        int IAuthorizationHandler.GetOrder() => -100;

        public Task<AuthorizationResult> AuthorizePublishingAsync(
            IClientInfo client, string streamPath, IDictionary<string, string> streamArguments, string publishingType)
        {
            if (_config.BlockPublishingWhenLimitReached && _podStatus.IsStreamsLimitReached)
            {
                _logger.StreamsLimitReached(client.ClientId);
                return Task.FromResult(AuthorizationResult.Unauthorized("Streams limit reached."));
            }

            if (_config.BlockPublishingWhenPendingStop && _podStatus.IsPendingStop)
            {
                _logger.PodPendingStop(client.ClientId);
                return Task.FromResult(AuthorizationResult.Unauthorized("Pod is pending stop."));
            }

            return Task.FromResult(AuthorizationResult.Authorized());
        }

        public Task<AuthorizationResult> AuthorizeSubscriptionAsync(
            IClientInfo client, string streamPath, IDictionary<string, string> streamArguments)
        {
            return Task.FromResult(AuthorizationResult.Authorized());
        }
    }
}
