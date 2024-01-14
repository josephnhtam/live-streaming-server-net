using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Newtorking.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Information, "Server is started | IPEndPoint: {IPEndPoint}")]
        public static partial void ServerStarted(this ILogger logger, string ipEndPoint);

        [LoggerMessage(LogLevel.Information, "Server is started")]
        public static partial void ServerStarted(this ILogger logger);

        [LoggerMessage(LogLevel.Information, "Server is stopped")]
        public static partial void ServerStopped(this ILogger logger);

        [LoggerMessage(LogLevel.Information, "Server is shutting down")]
        public static partial void ServerShuttingDown(this ILogger logger);

        [LoggerMessage(LogLevel.Error, "An error occurred while accepting a client connection")]
        public static partial void AcceptClientError(this ILogger logger, SocketException exception);

        [LoggerMessage(LogLevel.Error, "An error occurred in the server loop")]
        public static partial void ServerLoopError(this ILogger logger, Exception exception);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred in the client client loop")]
        public static partial void ClientLoopError(this ILogger logger, uint clientId, Exception exception);

        [LoggerMessage(LogLevel.Information, "ClientId: {ClientId} | Connected")]
        public static partial void ClientConnected(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Information, "ClientId: {ClientId} | Disconnected")]
        public static partial void ClientDisconnected(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred while sending data to the client")]
        public static partial void SendDataError(this ILogger logger, uint clientId, Exception exception);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | An error occurred while disposing outstanding buffer sender")]
        public static partial void OutstandingBufferSenderDisposeError(this ILogger logger, uint clientId, Exception exception);
    }
}
