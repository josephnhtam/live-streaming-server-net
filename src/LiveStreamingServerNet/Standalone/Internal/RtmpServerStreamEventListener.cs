using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.Standalone.Internal
{
    internal class RtmpServerStreamEventListener : IRtmpServerStreamEventHandler
    {
        public ValueTask OnRtmpStreamMetaDataReceived(uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnRtmpStreamPublishedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnRtmpStreamSubscribedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnRtmpStreamUnpublishedAsync(uint clientId, string streamPath)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnRtmpStreamUnsubscribedAsync(uint clientId, string streamPath)
        {
            throw new NotImplementedException();
        }
    }
}
