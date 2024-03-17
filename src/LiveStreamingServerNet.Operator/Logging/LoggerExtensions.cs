using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Operator.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "An error occurred when reconciling")]
        public static partial void ReconilingError(this ILogger logger, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred when creating a job")]
        public static partial void CreatingJobError(this ILogger logger, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred when patching a pod")]
        public static partial void PatchingPodError(this ILogger logger, Exception exception);
    }
}
