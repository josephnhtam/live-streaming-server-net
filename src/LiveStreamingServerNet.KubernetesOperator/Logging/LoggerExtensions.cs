using LiveStreamingServerNet.KubernetesOperator.Models;
using System.Text.Json;

namespace LiveStreamingServerNet.KubernetesOperator.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "An error occurred when reconciling")]
        public static partial void ReconilingError(this ILogger logger, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred when scaling the fleet")]
        public static partial void ScalingFleetError(this ILogger logger, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred when creating a pod")]
        public static partial void CreatingPodError(this ILogger logger, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred when patching the pod {PodName}")]
        public static partial void PatchingPodError(this ILogger logger, string podName, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred when deleting the pod {PodName}")]
        public static partial void DeletingPodError(this ILogger logger, string podName, Exception exception);

        public static void LogCurrentState(this ILogger logger, FleetState fleetState)
            => LogCurrentStateCore(logger, JsonSerializer.Serialize(fleetState));

        [LoggerMessage(LogLevel.Information, "Current State | {FleetState}")]
        private static partial void LogCurrentStateCore(this ILogger logger, string fleetState);

        public static void LogDesiredFleetStateChange(this ILogger logger, DesiredFleetStateChange desiredFleetStateChange)
            => LogDesiredFleetStateChangeCore(logger, JsonSerializer.Serialize(desiredFleetStateChange));

        [LoggerMessage(LogLevel.Information, "Desired State Change | {DesiredFleetStateChange}")]
        private static partial void LogDesiredFleetStateChangeCore(this ILogger logger, string desiredFleetStateChange);
    }
}
