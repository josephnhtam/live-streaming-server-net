using LiveStreamingServerNet.KubernetesPod.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.KubernetesPod.Internal.Services
{
    internal class StreamUnregistrationHandler : IRtmpServerStreamEventHandler
    {
        private readonly IStreamRegistry _streamRegistry;

        public const int Order = 100;
        int IRtmpServerStreamEventHandler.GetOrder() => Order;

        public StreamUnregistrationHandler(IStreamRegistry streamRegistry)
        {
            _streamRegistry = streamRegistry;
        }

        public async ValueTask OnRtmpStreamUnpublishedAsync(uint clientId, string streamPath)
        {
            await _streamRegistry.UnregsiterStreamAsync(streamPath);
        }

        public ValueTask OnRtmpStreamPublishedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpStreamSubscribedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpStreamUnsubscribedAsync(uint clientId, string streamPath)
            => ValueTask.CompletedTask;

        public ValueTask OnRtmpStreamMetaDataReceivedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData)
            => ValueTask.CompletedTask;
    }
}
