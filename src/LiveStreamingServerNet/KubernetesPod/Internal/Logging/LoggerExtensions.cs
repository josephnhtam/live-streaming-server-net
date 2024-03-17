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
    }
}
