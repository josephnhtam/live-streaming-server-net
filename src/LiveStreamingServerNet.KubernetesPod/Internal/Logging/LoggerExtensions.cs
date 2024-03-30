using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.KubernetesPod.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "An error occurred when watching the pod")]
        public static partial void WatchingPodError(this ILogger logger, Exception exception);

        [LoggerMessage(LogLevel.Information, "Stopping the pod gracefully")]
        public static partial void StoppingPodGracefully(this ILogger logger);

        [LoggerMessage(LogLevel.Error, "An error occurred when patching the pod | JsonPatch: {JsonPatch}")]
        public static partial void PatchingPodError(this ILogger logger, string jsonPatch, Exception exception);

        [LoggerMessage(LogLevel.Information, "Restarting watcher as no event is received since {LastEventTime}")]
        public static partial void RestartingWatcher(this ILogger logger, DateTimeOffset lastEventTime);

        [LoggerMessage(LogLevel.Information, "Ignoring error occurred when watching the pod")]
        public static partial void IgnoringWatchingPodError(this ILogger logger, Exception exception);

        [LoggerMessage(LogLevel.Warning, "ClientId: {ClientId} | Client IStreams limit reached")]
        public static partial void StreamsLimitReached(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Warning, "ClientId: {ClientId} | Pod is pending stop")]
        public static partial void PodPendingStop(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Information, "ClientId: {ClientId} | StreamPath: {StreamPath} | The stream has been registered")]
        public static partial void StreamRegistered(this ILogger logger, uint clientId, string streamPath);

        [LoggerMessage(LogLevel.Information, "ClientId: {ClientId} | StreamPath: {StreamPath} | The stream has been unregistered")]
        public static partial void StreamUnregistered(this ILogger logger, uint clientId, string streamPath);

        [LoggerMessage(LogLevel.Warning, "ClientId: {ClientId} | StreamPath: {StreamPath} | Reason: {Reason} | Failed to register the stream")]
        public static partial void StreamRegistrationFailed(this ILogger logger, uint clientId, string streamPath, string reason);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | StreamPath: {StreamPath} | Failed to unregister the stream")]
        public static partial void StreamUnregistrationFailed(this ILogger logger, uint clientId, string streamPath, Exception exception);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | StreamPath: {StreamPath} | An error occurred when registering the stream")]
        public static partial void RegisteringStreamError(this ILogger logger, uint clientId, string streamPath, Exception exception);

        [LoggerMessage(LogLevel.Trace, "ClientId: {ClientId} | StreamPath: {StreamPath} | Keepalive task started")]
        public static partial void KeepaliveTaskStarted(this ILogger logger, uint clientId, string streamPath);

        [LoggerMessage(LogLevel.Trace, "ClientId: {ClientId} | StreamPath: {StreamPath} | Keepalive task stopped")]
        public static partial void KeepaliveTaskStopped(this ILogger logger, uint clientId, string streamPath);

        [LoggerMessage(LogLevel.Debug, "ClientId: {ClientId} | StreamPath: {StreamPath} | Timestamp: {Timestamp} | The stream has been revalidated")]
        public static partial void StreamRevalidated(this ILogger logger, uint clientId, string streamPath, DateTime timestamp);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | StreamPath: {StreamPath} | Retryable: {Retryable} | Reason: {Reason} | Failed to revalidate the stream")]
        public static partial void RevalidatingStreamFailed(this ILogger logger, uint clientId, string streamPath, bool retryable, string reason);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | StreamPath: {StreamPath} | Failed to revalidate the stream within keepalive timeout period")]
        public static partial void RevalidatingStreamTimedOut(this ILogger logger, uint clientId, string streamPath);

        [LoggerMessage(LogLevel.Error, "ClientId: {ClientId} | StreamPath: {StreamPath} | An error occurred when revalidating the stream")]
        public static partial void RevalidatingStreamError(this ILogger logger, uint clientId, string streamPath, Exception exception);

        [LoggerMessage(LogLevel.Warning, "ClientId: {ClientId} | StreamPath: {StreamPath} | Disconnecting the client because of failure of keepalive")]
        public static partial void DisconnectingClientDueToKeepaliveFailure(this ILogger logger, uint clientId, string streamPath);
    }
}
