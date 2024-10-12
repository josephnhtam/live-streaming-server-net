using k8s;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.KubernetesPod.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Information, "Starting the pod watcher")]
        public static partial void StartingPodWatcher(this ILogger logger);

        [LoggerMessage(LogLevel.Information, "Pod event received (EventType={EventType})")]
        public static partial void PodEventReceived(this ILogger logger, WatchEventType eventType);

        [LoggerMessage(LogLevel.Error, "An error occurred when watching the pod")]
        public static partial void WatchingPodError(this ILogger logger, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred when watching the pod (RetryDelay={RetryDelay})")]
        public static partial void WatchingPodError(this ILogger logger, Exception exception, TimeSpan retryDelay);

        [LoggerMessage(LogLevel.Information, "Stopping the pod gracefully")]
        public static partial void StoppingPodGracefully(this ILogger logger);

        [LoggerMessage(LogLevel.Error, "An error occurred when patching the pod (JsonPatch={JsonPatch})")]
        public static partial void PatchingPodError(this ILogger logger, string jsonPatch, Exception exception);

        [LoggerMessage(LogLevel.Information, "Restarting the pod watcher as no event is received since (LastEventTime={LastEventTime})")]
        public static partial void RestartingPodWatcher(this ILogger logger, DateTimeOffset lastEventTime);

        [LoggerMessage(LogLevel.Information, "Ignoring error occurred when watching the pod")]
        public static partial void IgnoringWatchingPodError(this ILogger logger, Exception exception);

        [LoggerMessage(LogLevel.Warning, "Client IStreams limit reached (ClientId={ClientId})")]
        public static partial void StreamsLimitReached(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Warning, "Pod is pending stop (ClientId={ClientId})")]
        public static partial void PodPendingStop(this ILogger logger, uint clientId);

        [LoggerMessage(LogLevel.Information, "The stream has been registered (ClientId={ClientId}, StreamPath={StreamPath})")]
        public static partial void StreamRegistered(this ILogger logger, uint clientId, string streamPath);

        [LoggerMessage(LogLevel.Information, "The stream has been unregistered (ClientId={ClientId}, StreamPath={StreamPath})")]
        public static partial void StreamUnregistered(this ILogger logger, uint clientId, string streamPath);

        [LoggerMessage(LogLevel.Warning, "Failed to register the stream (ClientId={ClientId}, StreamPath={StreamPath}, Reason={Reason})")]
        public static partial void StreamRegistrationFailed(this ILogger logger, uint clientId, string streamPath, string reason);

        [LoggerMessage(LogLevel.Error, "Failed to unregister the stream (ClientId={ClientId}, StreamPath={StreamPath})")]
        public static partial void StreamUnregistrationFailed(this ILogger logger, uint clientId, string streamPath, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred when registering the stream (ClientId={ClientId}, StreamPath={StreamPath})")]
        public static partial void RegisteringStreamError(this ILogger logger, uint clientId, string streamPath, Exception exception);

        [LoggerMessage(LogLevel.Trace, "Keepalive task started (ClientId={ClientId}, StreamPath={StreamPath})")]
        public static partial void KeepaliveTaskStarted(this ILogger logger, uint clientId, string streamPath);

        [LoggerMessage(LogLevel.Trace, "Keepalive task stopped (ClientId={ClientId}, StreamPath={StreamPath})")]
        public static partial void KeepaliveTaskStopped(this ILogger logger, uint clientId, string streamPath);

        [LoggerMessage(LogLevel.Debug, "The stream has been revalidated (ClientId={ClientId}, StreamPath={StreamPath}, Timestamp={Timestamp})")]
        public static partial void StreamRevalidated(this ILogger logger, uint clientId, string streamPath, DateTime timestamp);

        [LoggerMessage(LogLevel.Error, "Failed to revalidate the stream (ClientId={ClientId}, StreamPath={StreamPath}, Retryable={Retryable}, Reason={Reason})")]
        public static partial void RevalidatingStreamFailed(this ILogger logger, uint clientId, string streamPath, bool retryable, string reason);

        [LoggerMessage(LogLevel.Error, "Failed to revalidate the stream within keepalive timeout period (ClientId={ClientId}, StreamPath={StreamPath})")]
        public static partial void RevalidatingStreamTimedOut(this ILogger logger, uint clientId, string streamPath);

        [LoggerMessage(LogLevel.Error, "An error occurred when revalidating the stream (ClientId={ClientId}, StreamPath={StreamPath})")]
        public static partial void RevalidatingStreamError(this ILogger logger, uint clientId, string streamPath, Exception exception);

        [LoggerMessage(LogLevel.Warning, "Disconnecting the client because of failure of keepalive (ClientId={ClientId}, StreamPath={StreamPath})")]
        public static partial void DisconnectingClientDueToKeepaliveFailure(this ILogger logger, uint clientId, string streamPath);
    }
}
