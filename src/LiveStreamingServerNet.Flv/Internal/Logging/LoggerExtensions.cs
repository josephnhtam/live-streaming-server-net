using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Flv.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "An error occurred while sending media message (ClientId={ClientId})")]
        public static partial void FailedToSendMediaMessage(this ILogger logger, string clientId, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred while writing FLV header (ClientId={ClientId})")]
        public static partial void FailedToWriteFlvHeader(this ILogger logger, string clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while writing FLV tag (ClientId={ClientId})")]
        public static partial void FailedToWriteFlvTag(this ILogger logger, string clientId, Exception ex);

        [LoggerMessage(LogLevel.Debug, "Begin media packet discard (ClientId={ClientId}, OutstandingSize={OutstandingSize}, OutstandingCount={OutstandingCount})")]
        public static partial void BeginMediaPacketDiscard(this ILogger logger, string clientId, long outstandingSize, long outstandingCount);

        [LoggerMessage(LogLevel.Debug, "End media packet discard (ClientId={ClientId}, OutstandingSize={OutstandingSize}, OutstandingCount={OutstandingCount})")]
        public static partial void EndMediaPacketDiscard(this ILogger logger, string clientId, long outstandingSize, long outstandingCount);

        [LoggerMessage(LogLevel.Error, "Stream readiness timeout (StreamPath={StreamPath})")]
        public static partial void ReadinessTimeout(this ILogger logger, string streamPath);
    }
}
