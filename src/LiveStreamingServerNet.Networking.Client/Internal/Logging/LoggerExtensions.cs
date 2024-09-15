using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Networking.Client.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Information, "Client is connected")]
        public static partial void ClientConnected(this ILogger logger);

        [LoggerMessage(LogLevel.Information, "Client is stopping")]
        public static partial void ClientStopping(this ILogger logger);

        [LoggerMessage(LogLevel.Information, "Client is stopped")]
        public static partial void ClientStopped(this ILogger logger);

        [LoggerMessage(LogLevel.Error, "An error occurred while running the client")]
        public static partial void ClientError(this ILogger logger, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching the client connected event")]
        public static partial void DispatchingClientConnectedEventError(this ILogger logger, Exception ex);

        [LoggerMessage(LogLevel.Error, "An error occurred while dispatching the client stopped event")]
        public static partial void DispatchingClientStoppedEventError(this ILogger logger, Exception ex);
    }
}
