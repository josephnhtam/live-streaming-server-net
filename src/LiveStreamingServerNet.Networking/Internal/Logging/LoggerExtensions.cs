using LiveStreamingServerNet.Networking.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Networking.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "An error occurred while writing to the send channel")]
        public static partial void BufferWritingError(this ILogger logger, Exception exception);

        [LoggerMessage(LogLevel.Error, "NetworkStreamWriter: {NetworkStreamWriter} | An error occurred while sending data to the client")]
        public static partial void SendDataError(this ILogger logger, INetworkStreamWriter networkStreamWriter, Exception exception);

        [LoggerMessage(LogLevel.Error, "NetworkStreamWriter: {NetworkStreamWriter} | An error occurred while disposing outstanding buffer sender")]
        public static partial void OutstandingBufferSenderDisposeError(this ILogger logger, INetworkStreamWriter networkStreamWriter, Exception exception);

        [LoggerMessage(LogLevel.Error, "SessionId: {SessionId} | An error occurred in the session loop")]
        public static partial void SessionLoopError(this ILogger logger, uint sessionId, Exception exception);

        [LoggerMessage(LogLevel.Error, "SessionId: {SessionId} | An error occurred while disposing the session")]
        public static partial void DisposeError(this ILogger logger, uint sessionId, Exception exception);

        [LoggerMessage(LogLevel.Error, "SessionId: {SessionId} | An error occurred while closing the TCP client")]
        public static partial void CloseTcpClientError(this ILogger logger, uint sessionId, Exception exception);
    }
}
