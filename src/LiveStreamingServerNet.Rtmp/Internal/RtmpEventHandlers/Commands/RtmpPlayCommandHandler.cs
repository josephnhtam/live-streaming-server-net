using LiveStreamingServerNet.Rtmp.Auth;
using LiveStreamingServerNet.Rtmp.Auth.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpPlayCommand(double TransactionId, IDictionary<string, object> CommandObject, string StreamName, double Start, double Duration, bool Reset);

    [RtmpCommand("play")]
    internal class RtmpPlayCommandHandler : RtmpCommandHandler<RtmpPlayCommand>
    {
        private readonly IServiceProvider _services;
        private readonly IRtmpServerContext _serverContext;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpMediaMessageCacherService _mediaMessageCacher;
        private readonly IRtmpServerStreamEventDispatcher _eventDispatch;
        private readonly ILogger<RtmpPlayCommandHandler> _logger;

        public RtmpPlayCommandHandler(
            IServiceProvider services,
            IRtmpServerContext serverContext,
            IRtmpStreamManagerService streamManager,
            IRtmpCommandMessageSenderService commandMessageSender,
            IRtmpMediaMessageCacherService mediaMessageCacher,
            IRtmpServerStreamEventDispatcher eventDispatch,
            ILogger<RtmpPlayCommandHandler> logger)
        {
            _services = services;
            _serverContext = serverContext;
            _streamManager = streamManager;
            _commandMessageSender = commandMessageSender;
            _mediaMessageCacher = mediaMessageCacher;
            _eventDispatch = eventDispatch;
            _logger = logger;
        }

        public override async ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpPlayCommand command,
            CancellationToken cancellationToken)
        {
            _logger.Play(clientContext.Client.ClientId, command.StreamName);

            if (clientContext.StreamId == null)
                throw new InvalidOperationException("Stream is not yet created.");

            var (streamPath, streamArguments) = ParseSubscriptionContext(command, clientContext);

            var authorizationResult = await AuthorizeAsync(clientContext, command, chunkStreamContext, streamPath, streamArguments);

            if (!authorizationResult.IsAuthorized)
                return false;

            streamPath = authorizationResult.StreamPathOverride ?? streamPath;
            streamArguments = authorizationResult.StreamArgumentsOverride ?? streamArguments;

            StartSubscribing(clientContext, command, chunkStreamContext, streamPath, streamArguments);
            return true;
        }

        private async ValueTask<AuthorizationResult> AuthorizeAsync(
            IRtmpClientContext clientContext,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            if (streamArguments.TryGetValue("code", out var authCode) && authCode == _serverContext.AuthCode)
                return AuthorizationResult.Authorized();

            foreach (var authorizationHandler in _services.GetServices<IAuthorizationHandler>().OrderBy(x => x.GetOrder()))
            {
                var result = await authorizationHandler.AuthorizeSubscriptionAsync(
                    clientContext.Client, streamPath, streamArguments);

                if (!result.IsAuthorized)
                    return result;

                streamPath = result.StreamPathOverride ?? streamPath;
                streamArguments = result.StreamArgumentsOverride ?? streamArguments;
            }

            return AuthorizationResult.Authorized(streamPath, streamArguments);
        }

        private static (string StreamPath, IReadOnlyDictionary<string, string> StreamArguments)
            ParseSubscriptionContext(RtmpPlayCommand command, IRtmpClientContext clientContext)
        {
            var (streamName, arguments) = StreamUtilities.ParseStreamPath(command.StreamName);

            var streamPath = $"/{string.Join('/',
                new string[] { clientContext.AppName, streamName }.Where(s => !string.IsNullOrEmpty(s)).ToArray())}";

            return (streamPath, arguments.AsReadOnly());
        }

        private async ValueTask<AuthorizationResult> AuthorizeAsync(
            IRtmpClientContext clientContext,
            RtmpPlayCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            var result = await AuthorizeAsync(clientContext, streamPath, streamArguments);

            if (!result.IsAuthorized)
            {
                _logger.AuthorizationFailed(clientContext.Client.ClientId, streamPath, result.Reason ?? "Unknown");
                await SendAuthorizationFailedCommandMessageAsync(clientContext, chunkStreamContext, result.Reason ?? "Unknown");
            }

            return result;
        }

        private bool StartSubscribing(
            IRtmpClientContext clientContext,
            RtmpPlayCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            var startSubscribingResult = _streamManager.StartSubscribingStream(clientContext, chunkStreamContext.ChunkStreamId, streamPath, streamArguments);

            switch (startSubscribingResult)
            {
                case SubscribingStreamResult.Succeeded:
                    _logger.SubscriptionStarted(clientContext.Client.ClientId, streamPath);
                    SendSubscriptionStartedMessage(clientContext, chunkStreamContext);
                    SendCachedStreamMessages(clientContext, chunkStreamContext);
                    CompleteSubscriptionInitialization(clientContext);
                    return true;

                case SubscribingStreamResult.AlreadySubscribing:
                    _logger.AlreadySubscribing(clientContext.Client.ClientId, streamPath);
                    SendBadConnectionCommandMessage(clientContext, chunkStreamContext, "Already subscribing.");
                    return false;

                case SubscribingStreamResult.AlreadyPublishing:
                    _logger.AlreadyPublishing(clientContext.Client.ClientId, streamPath);
                    SendBadConnectionCommandMessage(clientContext, chunkStreamContext, "Already publishing.");
                    return false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(startSubscribingResult), startSubscribingResult, null);
            }
        }

        private void SendCachedStreamMessages(IRtmpClientContext clientContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            var publishStreamContext = _streamManager.GetPublishStreamContext(clientContext.StreamSubscriptionContext!.StreamPath);

            if (publishStreamContext == null)
                return;

            _mediaMessageCacher.SendCachedStreamMetaDataMessage(
                clientContext, publishStreamContext,
                chunkStreamContext.MessageHeader.Timestamp,
                chunkStreamContext.MessageHeader.MessageStreamId);

            _mediaMessageCacher.SendCachedHeaderMessages(
                clientContext, publishStreamContext,
                chunkStreamContext.MessageHeader.Timestamp,
                chunkStreamContext.MessageHeader.MessageStreamId);

            if (publishStreamContext.GroupOfPicturesCacheActivated)
            {
                _mediaMessageCacher.SendCachedGroupOfPictures(
                    clientContext, publishStreamContext,
                    chunkStreamContext.MessageHeader.MessageStreamId);
            }
        }

        private void CompleteSubscriptionInitialization(IRtmpClientContext clientContext)
        {
            clientContext.StreamSubscriptionContext!.CompleteInitialization();

            _eventDispatch.RtmpStreamSubscribedAsync(
                clientContext,
                clientContext.StreamSubscriptionContext.StreamPath,
                clientContext.StreamSubscriptionContext.StreamArguments);
        }

        private async Task SendAuthorizationFailedCommandMessageAsync(IRtmpClientContext clientContext, IRtmpChunkStreamContext chunkStreamContext, string reason)
        {
            await _commandMessageSender.SendOnStatusCommandMessageAsync(
                clientContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishUnauthorized,
                reason);
        }

        private void SendSubscriptionStartedMessage(IRtmpClientContext clientContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            _commandMessageSender.SendOnStatusCommandMessageAsync(
                clientContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Status,
                RtmpStatusCodes.PlayStart,
                "Stream subscribed.");
        }

        private void SendBadConnectionCommandMessage(IRtmpClientContext clientContext, IRtmpChunkStreamContext chunkStreamContext, string reason)
        {
            _commandMessageSender.SendOnStatusCommandMessageAsync(
                clientContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PlayBadConnection,
                reason);
        }
    }
}
