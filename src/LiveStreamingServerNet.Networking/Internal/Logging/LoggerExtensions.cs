using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Networking.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "An error occurred while writing to the send channel")]
        public static partial void BufferWritingError(this ILogger logger, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred while sending data to the client (NetworkStreamWriter={NetworkStreamWriter})")]
        public static partial void SendDataError(this ILogger logger, INetworkStreamWriter networkStreamWriter, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred while disposing outstanding buffer sender (NetworkStreamWriter={NetworkStreamWriter})")]
        public static partial void OutstandingBufferSenderDisposeError(this ILogger logger, INetworkStreamWriter networkStreamWriter, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred in the session loop (SessionId={SessionId})")]
        public static partial void SessionLoopError(this ILogger logger, uint sessionId, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred while disposing the session (SessionId={SessionId})")]
        public static partial void DisposeError(this ILogger logger, uint sessionId, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred while closing the TCP client (SessionId={SessionId})")]
        public static partial void CloseTcpClientError(this ILogger logger, uint sessionId, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred while invoking the callback")]
        public static partial void CallbackInvocationError(this ILogger logger, Exception exception);
    }
}
