using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
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

            public IReadOnlyDictionary<string, object>? StreamMetaData { get; set; }

            public event EventHandler<IReadOnlyDictionary<string, object>>? OnStreamMetaDataUpdated;
            public event EventHandler<IRentedBuffer>? OnVideoDataReceived;
            public event EventHandler<IRentedBuffer>? OnAudioDataReceived;
            public event EventHandler<StatusEventArgs>? OnStatusReceived;

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
                subscribeStreamContext.OnStreamMetaDataUpdated += OnStreamContextMetaDataUpdated;
                subscribeStreamContext.OnVideoDataReceived += OnStreamContextVideoDataReceived;
                subscribeStreamContext.OnAudioDataReceived += OnStreamContextAudioDataReceived;
                subscribeStreamContext.OnStatusReceived += OnStreamContextStatusRecevied;
            }

            private void OnSubscribeContextRemoved(object? sender, IRtmpSubscribeStreamContext subscribeStreamContext)
            {
                subscribeStreamContext.OnStreamMetaDataUpdated -= OnStreamContextMetaDataUpdated;
                subscribeStreamContext.OnVideoDataReceived -= OnStreamContextVideoDataReceived;
                subscribeStreamContext.OnAudioDataReceived -= OnStreamContextAudioDataReceived;
                subscribeStreamContext.OnStatusReceived -= OnStreamContextStatusRecevied;
            }

            private void OnStreamContextMetaDataUpdated(object? sender, IReadOnlyDictionary<string, object> streamMetaData)
            {
                try
                {
                    StreamMetaData = streamMetaData;
                    OnStreamMetaDataUpdated?.Invoke(this, streamMetaData);
                }
                catch (Exception ex)
                {
                    _logger.StreamMetaDataUpdateError(_streamContext.StreamId, ex);
                }
            }

            private void OnStreamContextVideoDataReceived(object? sender, IRentedBuffer rentedBuffer)
            {
                try
                {
                    OnVideoDataReceived?.Invoke(this, rentedBuffer);
                }
                catch (Exception ex)
                {
                    _logger.VideoDataReceiveError(_streamContext.StreamId, ex);
                }
            }

            private void OnStreamContextAudioDataReceived(object? sender, IRentedBuffer rentedBuffer)
            {
                try
                {
                    OnAudioDataReceived?.Invoke(this, rentedBuffer);
                }
                catch (Exception ex)
                {
                    _logger.AudioDataReceiveError(_streamContext.StreamId, ex);
                }
            }

            private void OnStreamContextStatusRecevied(object? sender, StatusEventArgs e)
            {
                try
                {
                    OnStatusReceived?.Invoke(this, e);
                }
                catch (Exception ex)
                {
                    _logger.StatusReceiveError(_streamContext.StreamId, ex);
                }
            }
        }
    }
}