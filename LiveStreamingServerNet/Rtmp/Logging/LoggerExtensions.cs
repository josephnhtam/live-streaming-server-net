using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | Command ({commandName}) received")]
        public static partial void CommandReceived(this ILogger logger, uint peerId, string commandName);

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | Connect: {CommandObject}")]
        public static partial void Connect(this ILogger logger, uint peerId, IDictionary<string, object> commandObject);

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | Play: {StreamName}")]
        private static partial void PlayCore(this ILogger logger, uint peerId, string streamName);

        public static void Play(this ILogger logger, uint peerId, string streamName)
            => PlayCore(logger, peerId, !string.IsNullOrEmpty(streamName) ? streamName : "(Empty)");

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | StreamPath: {StreamPath} | Authorization failed")]
        public static partial void AuthorizationFailed(this ILogger logger, uint peerId, string streamPath);

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | StreamPath: {StreamPath} | Type:{Type} | Authorization failed")]
        public static partial void AuthorizationFailed(this ILogger logger, uint peerId, string streamPath, string type);

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | StreamPath: {StreamPath} | Start subscription successfully")]
        public static partial void SubscriptionStarted(this ILogger logger, uint peerId, string streamPath);

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | StreamPath: {StreamPath} | Already subscribing")]
        public static partial void AlreadySubscribing(this ILogger logger, uint peerId, string streamPath);

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | StreamPath: {StreamPath} | Already publishing")]
        public static partial void AlreadyPublishing(this ILogger logger, uint peerId, string streamPath);

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | StreamName: {StreamName} | Type: {PublishingType}")]
        private static partial void PublishCore(this ILogger logger, uint peerId, string streamName, string publishingType);

        public static void Publish(this ILogger logger, uint peerId, string streamName, string publishingType)
            => PublishCore(logger, peerId, !string.IsNullOrEmpty(streamName) ? streamName : "(Empty)", publishingType);

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | StreamPath: {StreamPath} | Type: {Type} | Start publishing successfully")]
        public static partial void PublishingStarted(this ILogger logger, uint peerId, string streamPath, string type);

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | StreamPath: {StreamPath} | Type: {Type} | Stream already exists")]
        public static partial void StreamAlreadyExists(this ILogger logger, uint peerId, string streamPath, string type);

        [LoggerMessage(LogLevel.Trace, "PeerId: {PeerId} | Acknowledgement received")]
        public static partial void AcknowledgementReceived(this ILogger logger, uint peerId);

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | SetChunkSize: {InChunkSize}")]
        public static partial void SetChunkSize(this ILogger logger, uint peerId, uint inChunkSize);

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | WindowAcknowledgementSize: {InWindowAcknowledgementSize}")]
        public static partial void WindowAcknowledgementSize(this ILogger logger, uint peerId, uint inWindowAcknowledgementSize);

        [LoggerMessage(LogLevel.Error, "PeerId: {PeerId} | Failed to handle chunk event")]
        public static partial void FailedToHandleChunkEvent(this ILogger logger, uint peerId);

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | Handshake C0 Handled")]
        public static partial void HandshakeC0Handled(this ILogger logger, uint peerId);

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | Handshake C1 Handled")]
        public static partial void HandshakeC1Handled(this ILogger logger, uint peerId);

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | Handshake C1 Handling Failed")]
        public static partial void HandshakeC1HandlingFailed(this ILogger logger, uint peerId);

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | Handshake C2 Handled")]
        public static partial void HandshakeC2Handled(this ILogger logger, uint peerId);

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | Handshake C2 Handling Failed")]
        public static partial void HandshakeC2HandlingFailed(this ILogger logger, uint peerId);

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | Handshake type: {HandshakeType}")]
        public static partial void HandshakeType(this ILogger logger, uint peerId, string handshakeType);

        [LoggerMessage(LogLevel.Error, "PeerId: {PeerId} | An error occurred while sending media message")]
        public static partial void FailedToSendMediaMessage(this ILogger logger, uint peerId, Exception exception);

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | Resume media package | Outstanding media message size: {OutstandingPackagesSize} | count: {OutstandingPackagesCount}")]
        public static partial void ResumeMediaPackage(this ILogger logger, uint peerId, long outstandingPackagesSize, long outstandingPackagesCount);

        [LoggerMessage(LogLevel.Debug, "PeerId: {PeerId} | Pause media package | Outstanding media message size: {OutstandingPackagesSize} | count: {OutstandingPackagesCount}")]
        public static partial void PauseMediaPackage(this ILogger logger, uint peerId, long outstandingPackagesSize, long outstandingPackagesCount);
    }
}
