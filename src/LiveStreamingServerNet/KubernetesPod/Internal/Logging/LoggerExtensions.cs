using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.KubernetesPod.Internal.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "An error occurred when watching the pod")]
        public static partial void ErrorWatchingPod(this ILogger logger, Exception exception);

        [LoggerMessage(LogLevel.Information, "Stopping the pod gracefully")]
        public static partial void StoppingPodGracefully(this ILogger logger);

        [LoggerMessage(LogLevel.Error, "An error occurred when patching the pod | JsonPatch: {JsonPatch}")]
        public static partial void ErrorPatchingPod(this ILogger logger, string jsonPatch, Exception exception);
    }
}
