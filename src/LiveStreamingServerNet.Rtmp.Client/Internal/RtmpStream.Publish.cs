using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal partial class RtmpStream : IRtmpStream
    {
        internal class RtmpPublishStream : IRtmpPublishStream
        {
            private readonly RtmpStream _stream;
            private readonly IRtmpStreamContext _streamContext;
            private readonly IRtmpCommanderService _commander;
            private readonly IRtmpMediaDataSenderService _mediaDataSender;
            private readonly ILogger _logger;

            public RtmpPublishStream(
                RtmpStream stream,
                IRtmpStreamContext streamContext,
                IRtmpCommanderService commander,
                IRtmpMediaDataSenderService mediaDataSender,
                ILogger logger)
            {
                _stream = stream;
                _streamContext = streamContext;
                _commander = commander;
                _mediaDataSender = mediaDataSender;
                _logger = logger;
            }

            public void Publish(string streamName)
            {
                Publish(streamName, "live");
            }

            public void Publish(string streamName, string type)
            {
                _stream.ValidateStreamNotDeleted();
                _commander.Publish(_streamContext.StreamId, streamName, type);
            }

            public ValueTask SendMetaDataAsync(IReadOnlyDictionary<string, object> metaData)
            {
                return _mediaDataSender.SendMetaDataAsync(GetPublishStreamContext(), metaData);
            }

            public ValueTask SendAudioDataAsync(IRentedBuffer payload, uint timestamp)
            {
                return _mediaDataSender.SendAudioDataAsync(GetPublishStreamContext(), payload, timestamp);
            }

            public ValueTask SendVideoDataAsync(IRentedBuffer payload, uint timestamp)
            {
                return _mediaDataSender.SendVideoDataAsync(GetPublishStreamContext(), payload, timestamp);
            }

            private IRtmpPublishStreamContext GetPublishStreamContext()
            {
                return _streamContext.PublishContext ?? throw new InvalidOperationException("The stream is not published.");
            }
        }
    }
}