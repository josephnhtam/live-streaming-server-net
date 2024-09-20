using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "SessionId: {SessionId} | An error occurred in the session loop")]
        public static partial void SessionLoopError(this ILogger logger, uint sessionId, Exception exception);

        [LoggerMessage(LogLevel.Debug, "SessionId: {SessionId} | Handshake S0 Handled")]
        public static partial void HandshakeS0Handled(this ILogger logger, uint sessionId);

        [LoggerMessage(LogLevel.Debug, "SessionId: {SessionId} | Handshake S1 Handled")]
        public static partial void HandshakeS1Handled(this ILogger logger, uint sessionId);

        [LoggerMessage(LogLevel.Error, "SessionId: {SessionId} | Handshake S1 Handling Failed")]
        public static partial void HandshakeS1HandlingFailed(this ILogger logger, uint sessionId);

        [LoggerMessage(LogLevel.Debug, "SessionId: {SessionId} | Handshake S2 Handled")]
        public static partial void HandshakeS2Handled(this ILogger logger, uint sessionId);

        [LoggerMessage(LogLevel.Error, "SessionId: {SessionId} | Handshake S2 Handling Failed")]
        public static partial void HandshakeS2HandlingFailed(this ILogger logger, uint sessionId);
    }
}