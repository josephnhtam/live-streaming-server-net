using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Exceptions;
using LiveStreamingServerNet.Rtmp.Client.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Client.Internal.Extensions;
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
        private bool _isDeleted;

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
            _commander = commander;
            Publish = new RtmpPublishStream(this, streamContext, chunkMessageSender, commander, logger);
            Subscribe = new RtmpSubscribeStream(this, streamContext, commander, logger);
        }

        public void Command(RtmpCommand command)
        {
            _commander.Command(command.ToInternal(StreamId));
        }

        public async Task<RtmpCommandResponse> CommandAsync(RtmpCommand command)
        {
            var tcs = new TaskCompletionSource<RtmpCommandResponse>();

            _commander.Command(
                command.ToInternal(StreamId),
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
    }
}