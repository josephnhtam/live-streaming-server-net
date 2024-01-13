using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Flv.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred while sending media message")]
        public static partial void FailedToSendMediaMessage(this ILogger logger, string clientId, Exception exception);

        [LoggerMessage(LogLevel.Debug, "ClientId: {ClientId} | Resume media package | Outstanding media message size: {OutstandingPackagesSize} | count: {OutstandingPackagesCount}")]
        public static partial void ResumeMediaPackage(this ILogger logger, string clientId, long outstandingPackagesSize, long outstandingPackagesCount);

        [LoggerMessage(LogLevel.Debug, "ClientId: {ClientId} | Pause media package | Outstanding media message size: {OutstandingPackagesSize} | count: {OutstandingPackagesCount}")]
        public static partial void PauseMediaPackage(this ILogger logger, string clientId, long outstandingPackagesSize, long outstandingPackagesCount);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred while writing FLV header")]
        public static partial void FailedToWriteFlvHeader(this ILogger logger, string clientId, Exception ex);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred while writing FLV tag")]
        public static partial void FailedToWriteFlvTag(this ILogger logger, string clientId, Exception ex);
    }
}
