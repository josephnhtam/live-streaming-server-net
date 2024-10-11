using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Flv.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred while sending media message")]
        public static partial void FailedToSendMediaMessage(this ILogger logger, string clientId, Exception exception);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred while writing FLV header")]
        public static partial void FailedToWriteFlvHeader(this ILogger logger, string clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred while writing FLV tag")]
        public static partial void FailedToWriteFlvTag(this ILogger logger, string clientId, Exception ex);

        [LoggerMessage(LogLevel.Debug, "ClientId: {ClientId} | Begin media packet discard | OutstandingSize: {OutstandingSize} | OutstandingCount: {OutstandingCount}")]
        public static partial void BeginMediaPacketDiscard(this ILogger logger, string clientId, long outstandingSize, long outstandingCount);

        [LoggerMessage(LogLevel.Debug, "ClientId: {ClientId} | End media packet discard | OutstandingSize: {OutstandingSize} | OutstandingCount: {OutstandingCount}")]
        public static partial void EndMediaPacketDiscard(this ILogger logger, string clientId, long outstandingSize, long outstandingCount);

    }
}
