using LiveStreamingServerNet.KubernetesOperator.Models;
using System.Text.Json;

namespace LiveStreamingServerNet.KubernetesOperator.Logging
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(LogLevel.Error, "An error occurred when reconciling")]
        public static partial void ReconilingError(this ILogger logger, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred when scaling the cluster")]
        public static partial void ScalingClusterError(this ILogger logger, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred when creating a pod")]
        public static partial void CreatingPodError(this ILogger logger, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred when patching the pod {PodName}")]
        public static partial void PatchingPodError(this ILogger logger, string podName, Exception exception);

        [LoggerMessage(LogLevel.Error, "An error occurred when deleting the pod {PodName}")]
        public static partial void DeletingPodError(this ILogger logger, string podName, Exception exception);

        public static void LogCurrentState(this ILogger logger, ClusterState clusterState)
            => LogCurrentStateCore(logger, JsonSerializer.Serialize(clusterState));

        [LoggerMessage(LogLevel.Information, "Current State | {ClusterState}")]
        private static partial void LogCurrentStateCore(this ILogger logger, string clusterState);

        public static void LogDesiredClusterStateChange(this ILogger logger, DesiredClusterStateChange desiredClusterStateChange)
            => LogDesiredClusterStateChangeCore(logger, JsonSerializer.Serialize(desiredClusterStateChange));

        [LoggerMessage(LogLevel.Information, "Desired State Change | {DesiredClusterStateChange}")]
        private static partial void LogDesiredClusterStateChangeCore(this ILogger logger, string desiredClusterStateChange);
    }
}
