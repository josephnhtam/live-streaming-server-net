using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Exceptions;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Client.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal
{
    using RtmpCommand = Client.Contracts.RtmpCommand;
    using RtmpCommandResponse = Client.Contracts.RtmpCommandResponse;

    internal partial class RtmpStream : IRtmpStream
    {
        private readonly IRtmpStreamContext _streamContext;
        private readonly IRtmpCommanderService _commander;
        private readonly ILogger<RtmpStream> _logger;

        public uint StreamId => _streamContext.StreamId;
        public IRtmpPublishStream Publish { get; }
        public IRtmpSubscribeStream Subscribe { get; }

        public event EventHandler<StatusEventArgs>? OnStatusReceived;
        public event EventHandler<UserControlEventArgs>? OnUserControlEventReceived;

        private bool _isDeleted;

        public RtmpStream(
            IRtmpStreamContext streamContext,
            IRtmpCommanderService commander,
            IRtmpMediaDataSenderService mediaDataSender,
            ILogger<RtmpStream> logger)
        {
            _streamContext = streamContext;
            _commander = commander;
            _logger = logger;
            Publish = new RtmpPublishStream(this, streamContext, commander, mediaDataSender, logger);
            Subscribe = new RtmpSubscribeStream(this, streamContext, commander, logger);

            _streamContext.OnStatusReceived += OnStreamContextStatusRecevied;
            _streamContext.OnUserControlEventReceived += OnStreamUserControlEventReceived;
        }

        public void Command(RtmpCommand command)
        {
            _commander.Command(command.ToInternal(StreamId, _streamContext.CommandChunkStreamId));
        }

        public async Task<RtmpCommandResponse> CommandAsync(RtmpCommand command)
        {
            var tcs = new TaskCompletionSource<RtmpCommandResponse>();

            _commander.Command(
                command.ToInternal(StreamId, _streamContext.CommandChunkStreamId),
                callback: (context, response) =>
                {
                    tcs.SetResult(response.ToExternal());
                    return Task.FromResult(true);
                },
                cancellationCallback: () => tcs.TrySetCanceled()
            );

            return await tcs.Task;
        }

        public void CloseStream()
        {
            ValidateStreamNotDeleted();
            _commander.CloseStream(StreamId);
        }

        public void DeleteStream()
        {
            _commander.DeleteStream(StreamId);
            _isDeleted = true;
        }

        private void ValidateStreamNotDeleted()
        {
            if (_isDeleted)
            {
                throw new RtmpStreamDeletedException();
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

        private void OnStreamUserControlEventReceived(object? sender, UserControlEventArgs e)
        {
            try
            {
                OnUserControlEventReceived?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                _logger.UserControlEventReceiveError(_streamContext.StreamId, ex);
            }
        }
    }
}