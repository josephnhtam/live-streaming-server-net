using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal class RtmpStream : IRtmpStream
    {
        private readonly IRtmpStreamContext _streamContext;
        private readonly IRtmpChunkMessageSenderService _chunkMessageSender;
        private readonly IRtmpCommanderService _commander;
        private readonly ILogger<RtmpStream> _logger;

        public uint StreamId => _streamContext.StreamId;

        public RtmpStream(
            IRtmpStreamContext streamContext,
            IRtmpChunkMessageSenderService chunkMessageSender,
            IRtmpCommanderService commander,
            ILogger<RtmpStream> logger)
        {
            _streamContext = streamContext;
            _chunkMessageSender = chunkMessageSender;
            _commander = commander;
            _logger = logger;
        }

        public void Play(string streamName)
        {
            Play(streamName, 0, 0, false);
        }

        public void Play(string streamName, double start, double duration, bool reset)
        {
            _commander.Play(_streamContext.StreamId, streamName, start, duration, reset);
        }
    }
}
