using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Server.Auth;
using LiveStreamingServerNet.Rtmp.Server.Internal.Authorization.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Logging;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Services.Extensions;
using LiveStreamingServerNet.Rtmp.Server.Internal.Utilities;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpPlayCommand(double TransactionId, IDictionary<string, object> CommandObject, string StreamName, double Start, double Duration, bool Reset);

    [RtmpCommand("play")]
    internal class RtmpPlayCommandHandler : RtmpCommandHandler<RtmpPlayCommand, IRtmpClientSessionContext>
    {
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpUserControlMessageSenderService _userControlMessageSender;
        private readonly IRtmpCacherService _cacher;
        private readonly IStreamAuthorization _streamAuthorization;
        private readonly ILogger<RtmpPlayCommandHandler> _logger;

        public RtmpPlayCommandHandler(
            IRtmpStreamManagerService streamManager,
            IRtmpCommandMessageSenderService commandMessageSender,
            IRtmpUserControlMessageSenderService userControlMessageSender,
            IRtmpCacherService cacher,
            IStreamAuthorization streamAuthorization,
            ILogger<RtmpPlayCommandHandler> logger)
        {
            _streamManager = streamManager;
            _commandMessageSender = commandMessageSender;
            _userControlMessageSender = userControlMessageSender;
            _cacher = cacher;
            _streamAuthorization = streamAuthorization;
            _logger = logger;
        }

        public override async ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientSessionContext clientContext,
            RtmpPlayCommand command,
            CancellationToken cancellationToken)
        {
            _logger.Play(clientContext.Client.Id, command.StreamName);

            var streamId = chunkStreamContext.MessageHeader.MessageStreamId;
            var streamContext = clientContext.GetStreamContext(streamId);

            if (streamContext == null)
            {
                _logger.StreamNotYetCreated(clientContext.Client.Id);
                return false;
            }

            var (streamPath, streamArguments) = ParseSubscriptionContext(command, clientContext);

            var authorizationResult = await AuthorizeAsync(streamContext, command, chunkStreamContext, streamPath, streamArguments);

            if (!authorizationResult.IsAuthorized)
                return false;

            streamPath = authorizationResult.StreamPathOverride ?? streamPath;
            streamArguments = authorizationResult.StreamArgumentsOverride ?? streamArguments;

            await StartSubscribingAsync(streamContext, command, chunkStreamContext, streamPath, streamArguments);
            return true;
        }

        private static (string StreamPath, IReadOnlyDictionary<string, string> StreamArguments)
            ParseSubscriptionContext(RtmpPlayCommand command, IRtmpClientSessionContext clientContext)
        {
            Debug.Assert(!string.IsNullOrEmpty(clientContext.AppName));

            var (streamName, arguments) = StreamUtilities.ParseStreamName(command.StreamName);
            var streamPath = StreamUtilities.ComposeStreamPath(clientContext.AppName, streamName);
            return (streamPath, arguments.AsReadOnly());
        }

        private async ValueTask<AuthorizationResult> AuthorizeAsync(
            IRtmpStreamContext streamContext,
            RtmpPlayCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            var result = await _streamAuthorization.AuthorizeSubscribingAsync(streamContext.ClientContext, streamPath, streamArguments);

            if (!result.IsAuthorized)
            {
                _logger.AuthorizationFailed(streamContext.ClientContext.Client.Id, streamPath, result.Reason ?? "Unknown");
                await SendAuthorizationFailedCommandMessageAsync(streamContext, chunkStreamContext, result.Reason ?? "Unknown");
            }

            return result;
        }

        private async ValueTask<bool> StartSubscribingAsync(
            IRtmpStreamContext streamContext,
            RtmpPlayCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments)
        {
            var startSubscribingResult = await _streamManager.StartSubscribingAsync(streamContext, streamPath, streamArguments);

            switch (startSubscribingResult.Result)
            {
                case SubscribingStreamResult.Succeeded:
                    _logger.SubscriptionStarted(streamContext.ClientContext.Client.Id, streamPath);
                    SendCachedStreamMessages(streamContext, chunkStreamContext, startSubscribingResult.PublishStreamContext);
                    CompleteSubscriptionInitialization(streamContext);
                    return true;

                case SubscribingStreamResult.AlreadySubscribing:
                    _logger.AlreadySubscribing(streamContext.ClientContext.Client.Id, streamPath);
                    return false;

                case SubscribingStreamResult.AlreadyPublishing:
                    _logger.AlreadyPublishing(streamContext.ClientContext.Client.Id, streamPath);
                    return false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(startSubscribingResult.Result), startSubscribingResult.Result, null);
            }
        }

        private void SendCachedStreamMessages(
            IRtmpStreamContext streamContext, IRtmpChunkStreamContext chunkStreamContext, IRtmpPublishStreamContext? publishStreamContext)
        {
            if (streamContext.SubscribeContext == null || publishStreamContext == null)
                return;

            _cacher.SendCachedStreamMetaDataMessage(
                streamContext.SubscribeContext, publishStreamContext);

            _cacher.SendCachedHeaderMessages(
                streamContext.SubscribeContext, publishStreamContext);

            if (publishStreamContext.GroupOfPicturesCacheActivated)
            {
                _cacher.SendCachedGroupOfPictures(
                    streamContext.SubscribeContext, publishStreamContext);
            }
        }

        private void CompleteSubscriptionInitialization(IRtmpStreamContext streamContext)
        {
            streamContext.SubscribeContext?.CompleteInitialization();
        }

        private async ValueTask SendAuthorizationFailedCommandMessageAsync(
            IRtmpStreamContext streamContext, IRtmpChunkStreamContext chunkStreamContext, string reason)
        {
            await _commandMessageSender.SendOnStatusCommandMessageAsync(
                streamContext,
                RtmpStatusLevels.Error,
                RtmpStreamStatusCodes.PlayFailed,
                reason);
        }
    }
}
