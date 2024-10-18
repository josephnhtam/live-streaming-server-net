using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "An error occurred in the client loop (ClientId={ClientId})")]
        public static partial void ClientLoopError(this ILogger logger, uint clientId, Exception exception);

        [LoggerMessage(LogLevel.Debug, "Command received (ClientId={ClientId}, commandName={CommandName})")]
        public static partial void CommandReceived(this ILogger logger, uint clientId, string commandName);

        [LoggerMessage(LogLevel.Debug, "Connect (ClientId={ClientId}, CommandObject={CommandObject})")]
        public static partial void Connect(this ILogger logger, uint clientId, IDictionary<string, object> commandObject);

        [LoggerMessage(LogLevel.Warning, "Client already connected (ClientId={ClientId})")]
        public static partial void ClientAlreadyConnected(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Warning, "Invalid app name (ClientId={ClientId})")]
        public static partial void InvalidAppName(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Debug, "Play (ClientId={ClientId}, StreamName={StreamName})")]
        private static partial void PlayCore(this ILogger logger, uint clientId, string streamName);

        public static void Play(this ILogger logger, uint clientId, string streamName)
            => PlayCore(logger, clientId, !string.IsNullOrEmpty(streamName) ? streamName : "(Empty)");

        [LoggerMessage(LogLevel.Warning, "Authorization failed (ClientId={ClientId}, StreamPath={StreamPath}, Reason={Reason})")]
        public static partial void AuthorizationFailed(this ILogger logger, uint clientId, string streamPath, string reason);

        [LoggerMessage(LogLevel.Warning, "Authorization failed (ClientId={ClientId}, StreamPath={StreamPath}, Type={Type}, Reason={Reason})")]
        public static partial void AuthorizationFailed(this ILogger logger, uint clientId, string streamPath, string type, string reason);

        [LoggerMessage(LogLevel.Information, "Start subscription successfully (ClientId={ClientId}, StreamPath={StreamPath})")]
        public static partial void SubscriptionStarted(this ILogger logger, uint clientId, string streamPath);

        [LoggerMessage(LogLevel.Debug, "Already subscribing (ClientId={ClientId}, StreamPath={StreamPath})")]
        public static partial void AlreadySubscribing(this ILogger logger, uint clientId, string streamPath);

        [LoggerMessage(LogLevel.Debug, "Already publishing (ClientId={ClientId}, StreamPath={StreamPath})")]
        public static partial void AlreadyPublishing(this ILogger logger, uint clientId, string streamPath);

        [LoggerMessage(LogLevel.Debug, "Publish (ClientId={ClientId}, StreamName={StreamName}, Type={PublishingType})")]
        private static partial void PublishCore(this ILogger logger, uint clientId, string streamName, string publishingType);

        public static void Publish(this ILogger logger, uint clientId, string streamName, string publishingType)
            => PublishCore(logger, clientId, !string.IsNullOrEmpty(streamName) ? streamName : "(Empty)", publishingType);

        [LoggerMessage(LogLevel.Information, "Start publishing successfully (ClientId={ClientId}, StreamPath={StreamPath}, Type={Type})")]
        public static partial void PublishingStarted(this ILogger logger, uint clientId, string streamPath, string type);

        [LoggerMessage(LogLevel.Debug, "Stream already exists (ClientId={ClientId}, StreamPath={StreamPath}, Type={Type})")]
        public static partial void StreamAlreadyExists(this ILogger logger, uint clientId, string streamPath, string type);

        [LoggerMessage(LogLevel.Warning, "Stream is not yet created (ClientId={ClientId})")]
        public static partial void StreamNotYetCreated(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Warning, "Stream is not yet created (ClientId={ClientId}, StreamId={StreamId})")]
        public static partial void PublishStreamNotYetCreated(this ILogger logger, uint clientId, uint streamId);

        [LoggerMessage(LogLevel.Trace, "Acknowledgement received (ClientId={ClientId})")]
        public static partial void AcknowledgementReceived(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Debug, "SetChunkSize (ClientId={ClientId}, InChunkSize={InChunkSize})")]
        public static partial void SetChunkSize(this ILogger logger, uint clientId, uint inChunkSize);

        [LoggerMessage(LogLevel.Debug, "WindowAcknowledgementSize (ClientId={ClientId}, InWindowAcknowledgementSize={InWindowAcknowledgementSize})")]
        public static partial void WindowAcknowledgementSize(this ILogger logger, uint clientId, uint inWindowAcknowledgementSize);

        [LoggerMessage(LogLevel.Error, "Failed to handle RTMP message (ClientId={ClientId})")]
        public static partial void FailedToHandleRtmpMessage(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Debug, "Handshake C0 Handled (ClientId={ClientId})")]
        public static partial void HandshakeC0Handled(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Debug, "Handshake C1 Handled (ClientId={ClientId})")]
        public static partial void HandshakeC1Handled(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Error, "Handshake C1 Handling Failed (ClientId={ClientId})")]
        public static partial void HandshakeC1HandlingFailed(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Debug, "Handshake C2 Handled (ClientId={ClientId})")]
        public static partial void HandshakeC2Handled(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Error, "Handshake C2 Handling Failed (ClientId={ClientId})")]
        public static partial void HandshakeC2HandlingFailed(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Debug, "Handshake type (ClientId={ClientId}, HandshakeType={HandshakeType})")]
        public static partial void HandshakeType(this ILogger logger, uint clientId, string handshakeType);

        [LoggerMessage(LogLevel.Error, "An error occurred while sending media message (ClientId={ClientId})")]
        public static partial void FailedToSendMediaMessage(this ILogger logger, uint clientId, Exception exception);

        [LoggerMessage(LogLevel.Debug, "Begin media packet discard (ClientId={ClientId}, OutstandingSize={OutstandingSize}, OutstandingCount={OutstandingCount})")]
        public static partial void BeginMediaPacketDiscard(this ILogger logger, uint clientId, long outstandingSize, long outstandingCount);

        [LoggerMessage(LogLevel.Debug, "End media packet discard (ClientId={ClientId}, OutstandingSize={OutstandingSize}, OutstandingCount={OutstandingCount})")]
        public static partial void EndMediaPacketDiscard(this ILogger logger, uint clientId, long outstandingSize, long outstandingCount);

        [LoggerMessage(LogLevel.Warning, "Reached max GOP cache size (StreamPath={StreamPath})")]
        public static partial void ReachedMaxGopCacheSize(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Warning, "Exceeded bandwidth limit (ClientId={ClientId})")]
        public static partial void ExceededBandwidthLimit(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching RTMP client connected event (ClientId={ClientId})")]
        public static partial void DispatchingRtmpClientConnectedEventError(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching RTMP client created event (ClientId={ClientId})")]
        public static partial void DispatchingRtmpClientCreatedEventError(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching RTMP client disposing event (ClientId={ClientId})")]
        public static partial void DispatchingRtmpClientDisposingEventError(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching RTMP client disposed event (ClientId={ClientId})")]
        public static partial void DispatchingRtmpClientDisposedEventError(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching RTMP client handshake complete event (ClientId={ClientId})")]
        public static partial void DispatchingRtmpClientHandshakeCompleteEventError(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching RTMP stream metadata received event (ClientId={ClientId})")]
        public static partial void DispatchingRtmpStreamMetaDataReceivedEventError(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching RTMP stream published event (ClientId={ClientId})")]
        public static partial void DispatchingRtmpStreamPublishedEventError(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching RTMP stream unpublished event (ClientId={ClientId})")]
        public static partial void DispatchingRtmpStreamUnpublishedEventError(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching RTMP stream subscribed event (ClientId={ClientId})")]
        public static partial void DispatchingRtmpStreamSubscribedEventError(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching RTMP stream unsubscribed event (ClientId={ClientId})")]
        public static partial void DispatchingRtmpStreamUnsubscribedEventError(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Warning, "Video codec not allowed (StreamPath={StreamPath}, VideoCodec={VideoCodec})")]
        public static partial void VideoCodecNotAllowed(this ILogger logger, string streamPath, VideoCodec videoCodec);

        [LoggerMessage(LogLevel.Warning, "Audio codec not allowed (StreamPath={StreamPath}, AudioCodec={AudioCodec})")]
        public static partial void AudioCodecNotAllowed(this ILogger logger, string streamPath, AudioCodec audioCodec);

        [LoggerMessage(LogLevel.Error, "An error occurred while initializing the upstream (StreamPath={StreamPath})")]
        public static partial void RtmpUpstreamInitializationError(this ILogger logger, string streamPath, Exception exception);
    }
}
