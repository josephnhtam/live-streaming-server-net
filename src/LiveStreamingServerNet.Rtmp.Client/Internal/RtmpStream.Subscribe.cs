using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    internal partial class RtmpStream : IRtmpStream
    {
        internal class RtmpSubscribeStream : IRtmpSubscribeStream
        {
            private readonly RtmpStream _stream;
            private readonly IRtmpStreamContext _streamContext;
            private readonly IRtmpCommanderService _commander;
            private readonly ILogger _logger;

            public IReadOnlyDictionary<string, object>? StreamMetaData { get; private set; }

            public event EventHandler<StreamMetaDataEventArgs>? OnStreamMetaDataReceived;
            public event EventHandler<MediaDataEventArgs>? OnVideoDataReceived;
            public event EventHandler<MediaDataEventArgs>? OnAudioDataReceived;

            public RtmpSubscribeStream(
                RtmpStream stream,
                IRtmpStreamContext streamContext,
                IRtmpCommanderService commander,
                ILogger logger)
            {
                _stream = stream;
                _streamContext = streamContext;
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
                _stream.ValidateStreamNotDeleted();
                _commander.Play(_streamContext.StreamId, streamName, start, duration, reset);
            }

            private void OnSubscribeContextCreated(object? sender, IRtmpSubscribeStreamContext subscribeStreamContext)
            {
                subscribeStreamContext.OnStreamMetaDataReceived += OnStreamContextMetaDataRecevied;
                subscribeStreamContext.OnVideoDataReceived += OnStreamContextVideoDataReceived;
                subscribeStreamContext.OnAudioDataReceived += OnStreamContextAudioDataReceived;
            }

            private void OnSubscribeContextRemoved(object? sender, IRtmpSubscribeStreamContext subscribeStreamContext)
            {
                subscribeStreamContext.OnStreamMetaDataReceived -= OnStreamContextMetaDataRecevied;
                subscribeStreamContext.OnVideoDataReceived -= OnStreamContextVideoDataReceived;
                subscribeStreamContext.OnAudioDataReceived -= OnStreamContextAudioDataReceived;
            }

            private void OnStreamContextMetaDataRecevied(object? sender, StreamMetaDataEventArgs e)
            {
                try
                {
                    StreamMetaData = e.StreamMetaData;
                    OnStreamMetaDataReceived?.Invoke(this, e);
                }
                catch (Exception ex)
                {
                    _logger.StreamMetaDataUpdateError(_streamContext.StreamId, ex);
                }
            }

            private void OnStreamContextVideoDataReceived(object? sender, MediaDataEventArgs e)
            {
                try
                {
                    OnVideoDataReceived?.Invoke(this, e);
                }
                catch (Exception ex)
                {
                    _logger.VideoDataReceiveError(_streamContext.StreamId, ex);
                }
            }

            private void OnStreamContextAudioDataReceived(object? sender, MediaDataEventArgs e)
            {
                try
                {
                    OnAudioDataReceived?.Invoke(this, e);
                }
                catch (Exception ex)
                {
                    _logger.AudioDataReceiveError(_streamContext.StreamId, ex);
                }
            }
        }
    }
}