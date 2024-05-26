using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred in the client loop")]
        public static partial void ClientLoopError(this ILogger logger, uint clientId, Exception exception);

        [LoggerMessage(LogLevel.Debug, "ClientId: {ClientId} | Command ({commandName}) received")]
        public static partial void CommandReceived(this ILogger logger, uint clientId, string commandName);

        [LoggerMessage(LogLevel.Debug, "ClientId: {ClientId} | Connect: {CommandObject}")]
        public static partial void Connect(this ILogger logger, uint clientId, IDictionary<string, object> commandObject);

        [LoggerMessage(LogLevel.Warning, "ClientId: {ClientId} | Client already connected")]
        public static partial void ClientAlreadyConnected(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Warning, "ClientId: {ClientId} | Invalid app name")]
        public static partial void InvalidAppName(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Debug, "ClientId: {ClientId} | Play: {StreamName}")]
        private static partial void PlayCore(this ILogger logger, uint clientId, string streamName);

        public static void Play(this ILogger logger, uint clientId, string streamName)
            => PlayCore(logger, clientId, !string.IsNullOrEmpty(streamName) ? streamName : "(Empty)");

        [LoggerMessage(LogLevel.Warning, "ClientId: {ClientId} | StreamPath: {StreamPath} | Reason: {Reason} | Authorization failed")]
        public static partial void AuthorizationFailed(this ILogger logger, uint clientId, string streamPath, string reason);

        [LoggerMessage(LogLevel.Warning, "ClientId: {ClientId} | StreamPath: {StreamPath} | Type:{Type} | Reason: {Reason} | Authorization failed")]
        public static partial void AuthorizationFailed(this ILogger logger, uint clientId, string streamPath, string type, string reason);

        [LoggerMessage(LogLevel.Information, "ClientId: {ClientId} | StreamPath: {StreamPath} | Start subscription successfully")]
        public static partial void SubscriptionStarted(this ILogger logger, uint clientId, string streamPath);

        [LoggerMessage(LogLevel.Debug, "ClientId: {ClientId} | StreamPath: {StreamPath} | Already subscribing")]
        public static partial void AlreadySubscribing(this ILogger logger, uint clientId, string streamPath);

        [LoggerMessage(LogLevel.Debug, "ClientId: {ClientId} | StreamPath: {StreamPath} | Already publishing")]
        public static partial void AlreadyPublishing(this ILogger logger, uint clientId, string streamPath);

        [LoggerMessage(LogLevel.Debug, "ClientId: {ClientId} | StreamName: {StreamName} | Type: {PublishingType}")]
        private static partial void PublishCore(this ILogger logger, uint clientId, string streamName, string publishingType);

        public static void Publish(this ILogger logger, uint clientId, string streamName, string publishingType)
            => PublishCore(logger, clientId, !string.IsNullOrEmpty(streamName) ? streamName : "(Empty)", publishingType);

        [LoggerMessage(LogLevel.Information, "ClientId: {ClientId} | StreamPath: {StreamPath} | Type: {Type} | Start publishing successfully")]
        public static partial void PublishingStarted(this ILogger logger, uint clientId, string streamPath, string type);

        [LoggerMessage(LogLevel.Debug, "ClientId: {ClientId} | StreamPath: {StreamPath} | Type: {Type} | Stream already exists")]
        public static partial void StreamAlreadyExists(this ILogger logger, uint clientId, string streamPath, string type);

        [LoggerMessage(LogLevel.Warning, "ClientId: {ClientId} | Stream is not yet created")]
        public static partial void StreamNotYetCreated(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Trace, "ClientId: {ClientId} | Acknowledgement received")]
        public static partial void AcknowledgementReceived(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Debug, "ClientId: {ClientId} | SetChunkSize: {InChunkSize}")]
        public static partial void SetChunkSize(this ILogger logger, uint clientId, uint inChunkSize);

        [LoggerMessage(LogLevel.Debug, "ClientId: {ClientId} | WindowAcknowledgementSize: {InWindowAcknowledgementSize}")]
        public static partial void WindowAcknowledgementSize(this ILogger logger, uint clientId, uint inWindowAcknowledgementSize);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | Failed to handle chunk event")]
        public static partial void FailedToHandleChunkEvent(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Debug, "ClientId: {ClientId} | Handshake C0 Handled")]
        public static partial void HandshakeC0Handled(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Debug, "ClientId: {ClientId} | Handshake C1 Handled")]
        public static partial void HandshakeC1Handled(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | Handshake C1 Handling Failed")]
        public static partial void HandshakeC1HandlingFailed(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Debug, "ClientId: {ClientId} | Handshake C2 Handled")]
        public static partial void HandshakeC2Handled(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | Handshake C2 Handling Failed")]
        public static partial void HandshakeC2HandlingFailed(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Debug, "ClientId: {ClientId} | Handshake type: {HandshakeType}")]
        public static partial void HandshakeType(this ILogger logger, uint clientId, string handshakeType);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred while sending media message")]
        public static partial void FailedToSendMediaMessage(this ILogger logger, uint clientId, Exception exception);

        [LoggerMessage(LogLevel.Warning, "ClientId: {ClientId} | Resume media package | Outstanding media message size: {OutstandingPackagesSize} | count: {OutstandingPackagesCount}")]
        public static partial void ResumeMediaPackage(this ILogger logger, uint clientId, long outstandingPackagesSize, long outstandingPackagesCount);

        [LoggerMessage(LogLevel.Warning, "ClientId: {ClientId} | Pause media package | Outstanding media message size: {OutstandingPackagesSize} | count: {OutstandingPackagesCount}")]
        public static partial void PauseMediaPackage(this ILogger logger, uint clientId, long outstandingPackagesSize, long outstandingPackagesCount);

        [LoggerMessage(LogLevel.Warning, "StreamPath: {StreamPath} | Reached max GOP cache size")]
        public static partial void ReachedMaxGopCacheSize(this ILogger logger, string streamPath);

        [LoggerMessage(LogLevel.Warning, "ClientId: {ClientId} | Exceeded bandwidth limit")]
        public static partial void ExceededBandwidthLimit(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred while dispatching RTMP client connected event")]
        public static partial void DispatchingRtmpClientConnectedEventError(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred while dispatching RTMP client created event")]
        public static partial void DispatchingRtmpClientCreatedEventError(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred while dispatching RTMP client disposed event")]
        public static partial void DispatchingRtmpClientDisposedEventError(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred while dispatching RTMP client handshake complete event")]
        public static partial void DispatchingRtmpClientHandshakeCompleteEventError(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred while dispatching RTMP stream metadata recevied event")]
        public static partial void DispatchingRtmpStreamMetaDataReceivedEventError(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred while dispatching RTMP stream published event")]
        public static partial void DispatchingRtmpStreamPublishedEventError(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred while dispatching RTMP stream unpublished event")]
        public static partial void DispatchingRtmpStreamUnpublishedEventError(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred while dispatching RTMP stream subscribed event")]
        public static partial void DispatchingRtmpStreamSubscribedEventError(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred while dispatching RTMP stream unsubscribed event")]
        public static partial void DispatchingRtmpStreamUnsubscribedEventError(this ILogger logger, uint clientId, Exception ex);

        [LoggerMessage(LogLevel.Warning, "ClientId: {ClientId} | StreamPath: {StreamPath} | VideoCodec: {VideoCodec} | Video codec not allowed")]
        public static partial void VideoCodecNotAllowed(this ILogger logger, uint clientId, string streamPath, VideoCodec videoCodec);

        [LoggerMessage(LogLevel.Warning, "ClientId: {ClientId} | StreamPath: {StreamPath} | AudioCodec: {AudioCodec} | Audio codec not allowed")]
        public static partial void AudioCodecNotAllowed(this ILogger logger, uint clientId, string streamPath, AudioCodec audioCodec);
    }
}
