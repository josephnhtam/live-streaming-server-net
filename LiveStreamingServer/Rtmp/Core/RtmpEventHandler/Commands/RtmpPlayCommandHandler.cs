using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.Services.Contracts;
using LiveStreamingServer.Rtmp.Core.Services.Extensions;
using LiveStreamingServer.Rtmp.Core.Utilities;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.Commands
{
    public record RtmpPlayCommand(double TransactionId, IDictionary<string, object> CommandObject, string StreamName, double Start, double Duration, bool Reset);

    [RtmpCommand("play")]
    public class RtmpPlayCommandHandler : RtmpCommandHandler<RtmpPlayCommand>
    {
        private readonly IRtmpServerContext _serverContext;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpMediaMessageSenderService _mediaMessageSender;
        private readonly ILogger<RtmpPlayCommandHandler> _logger;

        public RtmpPlayCommandHandler(
            IRtmpServerContext serverContext,
            IRtmpCommandMessageSenderService commandMessageSender,
            IRtmpMediaMessageSenderService mediaMessageSender,
            ILogger<RtmpPlayCommandHandler> logger)
        {
            _serverContext = serverContext;
            _commandMessageSender = commandMessageSender;
            _mediaMessageSender = mediaMessageSender;
            _logger = logger;
        }

        public override async Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
            RtmpPlayCommand command,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("PeerId: {PeerId} | Play: {StreamName}",
                peerContext.Peer.PeerId, !string.IsNullOrEmpty(command.StreamName) ? command.StreamName : "(Empty)");

            var (streamPath, streamArguments) = ParseSubscriptionContext(command, peerContext);

            if (await AuthorizeAsync(peerContext, command, chunkStreamContext, streamPath, streamArguments))
            {
                StartSubscribing(peerContext, command, chunkStreamContext, streamPath, streamArguments);
            }

            return true;
        }

        private Task<bool> AuthorizeAsync(IRtmpClientPeerContext peerContext, string streamPath, IDictionary<string, string> streamArguments)
        {
            return Task.FromResult(true);
        }

        private static (string StreamPath, IDictionary<string, string> StreamArguments)
            ParseSubscriptionContext(RtmpPlayCommand command, IRtmpClientPeerContext peerContext)
        {
            var (streamName, arguments) = StreamUtilities.ParseStreamPath(command.StreamName);

            var streamPath = $"/{string.Join('/',
                new string[] { peerContext.AppName, streamName }.Where(s => !string.IsNullOrEmpty(s)).ToArray())}";

            return (streamPath, arguments);
        }

        private async Task<bool> AuthorizeAsync(
            IRtmpClientPeerContext peerContext,
            RtmpPlayCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IDictionary<string, string> streamArguments)
        {
            if (!await AuthorizeAsync(peerContext, streamPath, streamArguments))
            {
                _logger.LogWarning("PeerId: {PeerId} | PublishStreamPath: {PublishStreamPath} | Authorization failed",
                    peerContext.Peer.PeerId, streamPath);

                SendAuthorizationFailedCommandMessage(peerContext, chunkStreamContext);
                return false;
            }

            return true;
        }

        private bool StartSubscribing(
            IRtmpClientPeerContext peerContext,
            RtmpPlayCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IDictionary<string, string> streamArguments)
        {
            var startSubscribingResult = _serverContext.StartSubscribingStream(peerContext, chunkStreamContext.ChunkStreamId, streamPath, streamArguments);

            switch (startSubscribingResult)
            {
                case SubscribingStreamResult.Succeeded:
                    _logger.LogInformation("PeerId: {PeerId} | PublishStreamPath: {PublishStreamPath} | Start subscription successfully",
                        peerContext.Peer.PeerId, streamPath);
                    SendSubscriptionStartedMessage(peerContext, chunkStreamContext);
                    SendCachedHeaderMessages(peerContext, chunkStreamContext);
                    return true;

                case SubscribingStreamResult.AlreadySubscribing:
                    _logger.LogWarning("PeerId: {PeerId} | PublishStreamPath: {PublishStreamPath} | Already subscribing",
                        peerContext.Peer.PeerId, streamPath);
                    SendAlreadySubscribingCommandMessage(peerContext, chunkStreamContext);
                    return false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(startSubscribingResult), startSubscribingResult, null);
            }
        }

        private void SendCachedHeaderMessages(IRtmpClientPeerContext peerContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            var publishStreamContext = _serverContext
                .GetPublishingClientPeerContext(peerContext.StreamSubscriptionContext!.StreamPath)?
                .PublishStreamContext;

            if (publishStreamContext == null)
                return;

            var videoSequenceHeader = publishStreamContext.VideoSequenceHeader;
            if (videoSequenceHeader != null)
            {
                _mediaMessageSender.SendVideoMessage(peerContext, chunkStreamContext, (netBuffer) =>
                    netBuffer.Write(videoSequenceHeader)
                );
            }

            var audioSequenceHeader = publishStreamContext.AudioSequenceHeader;
            if (audioSequenceHeader != null)
            {
                _mediaMessageSender.SendAudioMessage(peerContext, chunkStreamContext, (netBuffer) =>
                    netBuffer.Write(audioSequenceHeader)
                );
            }
        }

        private void SendAuthorizationFailedCommandMessage(IRtmpClientPeerContext peerContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            _commandMessageSender.SendOnStatusCommandMessageAsync(
                peerContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishUnauthorized,
                "Authorization failed.");
        }

        private void SendSubscriptionStartedMessage(IRtmpClientPeerContext peerContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            _commandMessageSender.SendOnStatusCommandMessageAsync(
                peerContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Status,
                RtmpStatusCodes.PlayStart,
                "Stream subscribed.");
        }

        private void SendAlreadySubscribingCommandMessage(IRtmpClientPeerContext peerContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            _commandMessageSender.SendOnStatusCommandMessageAsync(
                peerContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PlayBadConnection,
                "Already subscribing.");
        }
    }
}
