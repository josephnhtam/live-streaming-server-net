using LiveStreamingServerNet.KubernetesPod.Internal.Services.Contracts;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.KubernetesPod.Internal.Services
{
    internal class PodEventListener : IRtmpServerConnectionEventHandler, IRtmpServerStreamEventHandler
    {
        private readonly IPodLifetimeManager _podLifetimeManager;

        int IRtmpServerConnectionEventHandler.GetOrder() => -100;
        int IRtmpServerStreamEventHandler.GetOrder() => -100;

        public PodEventListener(IPodLifetimeManager podLifetimeManager)
        {
            _podLifetimeManager = podLifetimeManager;
        }

        public async ValueTask OnRtmpClientDisposedAsync(uint clientId)
        {
            await _podLifetimeManager.OnClientDisposedAsync(clientId);
        }

        public async ValueTask OnRtmpStreamPublishedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            await _podLifetimeManager.OnStreamPublishedAsync(clientId, streamPath);
        }

        public async ValueTask OnRtmpStreamUnpublishedAsync(uint clientId, string streamPath)
        {
            await _podLifetimeManager.OnStreamUnpublishedAsync(clientId, streamPath);
        }

        public ValueTask OnRtmpClientCreatedAsync(IClientControl client)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpClientConnectedAsync(
            uint clientId,
            IReadOnlyDictionary<string, object> commandObject,
            IReadOnlyDictionary<string, object>? arguments)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpClientHandshakeCompleteAsync(uint clientId)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpStreamSubscribedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpStreamUnsubscribedAsync(uint clientId, string streamPath)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpStreamMetaDataReceivedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData)
            => ValueTask.CompletedTask;
    }
}
