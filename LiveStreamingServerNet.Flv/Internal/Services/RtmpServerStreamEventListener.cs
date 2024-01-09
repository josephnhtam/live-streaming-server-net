using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class RtmpServerStreamEventListener : IRtmpServerStreamEventHandler
    {
        public Task OnRtmpStreamMetaDataReceived(uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            return Task.CompletedTask;
        }

        public Task OnRtmpStreamPublishedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            return Task.CompletedTask;
        }

        public Task OnRtmpStreamSubscribedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            return Task.CompletedTask;
        }

        public Task OnRtmpStreamUnpublishedAsync(uint clientId, string streamPath)
        {
            return Task.CompletedTask;
        }

        public Task OnRtmpStreamUnsubscribedAsync(uint clientId, string streamPath)
        {
            return Task.CompletedTask;
        }
    }
}
