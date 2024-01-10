using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class RtmpServerStreamEventListener : IRtmpServerStreamEventHandler
    {
        private readonly IFlvStreamManagerService _streamManager;

        public RtmpServerStreamEventListener(IFlvStreamManagerService streamManager)
        {
            _streamManager = streamManager;
        }

        public Task OnRtmpStreamPublishedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var streamContext = new FlvStreamContext(streamPath, streamArguments.ToDictionary());
            _streamManager.StartPublishingStream(streamContext);
            return Task.CompletedTask;
        }

        public Task OnRtmpStreamUnpublishedAsync(uint clientId, string streamPath)
        {
            _streamManager.StopPublishingStream(streamPath, out _);
            return Task.CompletedTask;
        }

        public Task OnRtmpStreamMetaDataReceived(uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            var streamContext = _streamManager.GetFlvStreamContext(streamPath);

            if (streamContext != null)
                streamContext.StreamMetaData = metaData.ToDictionary();

            return Task.CompletedTask;
        }

        public Task OnRtmpStreamSubscribedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            return Task.CompletedTask;
        }

        public Task OnRtmpStreamUnsubscribedAsync(uint clientId, string streamPath)
        {
            return Task.CompletedTask;
        }
    }
}
