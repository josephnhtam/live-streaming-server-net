﻿using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
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

        public ValueTask OnRtmpStreamPublishedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            var streamContext = new FlvStreamContext(streamPath, new Dictionary<string, string>(streamArguments));
            _streamManager.StartPublishingStream(streamContext);
            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpStreamUnpublishedAsync(uint clientId, string streamPath)
        {
            if (_streamManager.StopPublishingStream(streamPath, out var existingSubscribers))
            {
                foreach (var subscriber in existingSubscribers)
                    subscriber.Stop();
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpStreamMetaDataReceived(uint clientId, string streamPath, IReadOnlyDictionary<string, object> metaData)
        {
            var streamContext = _streamManager.GetFlvStreamContext(streamPath);

            if (streamContext != null)
                streamContext.StreamMetaData = new Dictionary<string, object>(metaData);

            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpStreamSubscribedAsync(uint clientId, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask OnRtmpStreamUnsubscribedAsync(uint clientId, string streamPath)
        {
            return ValueTask.CompletedTask;
        }
    }
}
