using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal class RtmpStream : IRtmpStream
    {
        private readonly IRtmpStreamContext _streamContext;

        public uint StreamId => _streamContext.StreamId;
        public IRtmpPublishStream Publish { get; }
        public IRtmpSubscribeStream Subscribe { get; }

        public RtmpStream(
            IRtmpStreamContext streamContext,
            IRtmpChunkMessageSenderService chunkMessageSender,
            IRtmpCommanderService commander,
            ILogger<RtmpStream> logger)
        {
            _streamContext = streamContext;

            Publish = new RtmpPublishStream(streamContext, chunkMessageSender, commander, logger);
            Subscribe = new RtmpSubscribeStream(streamContext, chunkMessageSender, commander, logger);
        }

        internal class RtmpPublishStream : IRtmpPublishStream
        {
            private readonly IRtmpStreamContext _streamContext;
            private readonly IRtmpChunkMessageSenderService _chunkMessageSender;
            private readonly IRtmpCommanderService _commander;
            private readonly ILogger _logger;

            public RtmpPublishStream(
                IRtmpStreamContext streamContext,
                IRtmpChunkMessageSenderService chunkMessageSender,
                IRtmpCommanderService commander,
                ILogger logger)
            {
                _streamContext = streamContext;
                _chunkMessageSender = chunkMessageSender;
                _commander = commander;
                _logger = logger;
            }
        }

        internal class RtmpSubscribeStream : IRtmpSubscribeStream
        {
            private readonly IRtmpStreamContext _streamContext;
            private readonly IRtmpChunkMessageSenderService _chunkMessageSender;
            private readonly IRtmpCommanderService _commander;
            private readonly ILogger _logger;
            public IReadOnlyDictionary<string, object>? StreamMetaData { get; set; }
            public event EventHandler<IReadOnlyDictionary<string, object>>? OnStreamMetaDataUpdated;

            public RtmpSubscribeStream(
                IRtmpStreamContext streamContext,
                IRtmpChunkMessageSenderService chunkMessageSender,
                IRtmpCommanderService commander,
                ILogger logger)
            {
                _streamContext = streamContext;
                _chunkMessageSender = chunkMessageSender;
                _commander = commander;
                _logger = logger;

                _streamContext.OnSubscribeContextCreated += OnSubscribeContextCreated;
                _streamContext.OnSubscribeContextRemoved += OnSubscribeContextRemoved;
            }

            public void Play(string streamName)
            {
                Play(streamName, 0, 0, false);
            }

            public void Play(string streamName, double start, double duration, bool reset)
            {
                _commander.Play(_streamContext.StreamId, streamName, start, duration, reset);
            }

            private void OnSubscribeContextCreated(object? sender, IRtmpSubscribeStreamContext subscribeStreamContext)
            {
                subscribeStreamContext.OnStreamMetaDataUpdated += OnStreamContextMetaDataUpdated;
            }

            private void OnSubscribeContextRemoved(object? sender, IRtmpSubscribeStreamContext subscribeStreamContext)
            {
                subscribeStreamContext.OnStreamMetaDataUpdated -= OnStreamContextMetaDataUpdated;
            }

            private void OnStreamContextMetaDataUpdated(object? sender, IReadOnlyDictionary<string, object> streamMetaData)
            {
                StreamMetaData = streamMetaData;
                OnStreamMetaDataUpdated?.Invoke(this, streamMetaData);
            }
        }
    }
}
