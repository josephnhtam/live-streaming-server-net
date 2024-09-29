using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal partial class RtmpStream : IRtmpStream
    {
        internal class RtmpPublishStream : IRtmpPublishStream
        {
            private readonly RtmpStream _stream;
            private readonly IRtmpStreamContext _streamContext;
            private readonly IRtmpChunkMessageSenderService _chunkMessageSender;
            private readonly IRtmpCommanderService _commander;
            private readonly ILogger _logger;

            public RtmpPublishStream(
                RtmpStream stream,
                IRtmpStreamContext streamContext,
                IRtmpChunkMessageSenderService chunkMessageSender,
                IRtmpCommanderService commander,
                ILogger logger)
            {
                _stream = stream;
                _streamContext = streamContext;
                _chunkMessageSender = chunkMessageSender;
                _commander = commander;
                _logger = logger;
            }
        }
    }
}