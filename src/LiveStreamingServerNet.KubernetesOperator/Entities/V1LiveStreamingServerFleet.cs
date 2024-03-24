using k8s.Models;
using KubeOps.Abstractions.Entities;
using KubeOps.Abstractions.Entities.Attributes;

namespace LiveStreamingServerNet.KubernetesOperator.Entities
{
    [KubernetesEntity(Group = "live-streaming-server.net", ApiVersion = "v1", Kind = "Fleet", PluralName = "Fleets")]
    public partial class V1LiveStreamingServerFleet : CustomKubernetesEntity<V1LiveStreamingServerFleet.EntitySpec, V1LiveStreamingServerFleet.EntityStatus>
    {
        public class EntitySpec
        {
            [RangeMinimum(1), Description("The minimum number of replicas.")]
            public int MinReplicas { get; set; } = 1;
            [RangeMinimum(1), Description("The maximum number of replicas.")]
            public int MaxReplicas { get; set; } = 10;

            [RangeMinimum(1), Description("The maximum number of streams that a single pod can publish.")]
            public int PodStreamsLimit { get; set; } = 4;
            [RangeMinimum(0), RangeMaximum(1, true), Description("The target utilization rate for the pod.")]
            public float TargetUtilization { get; set; } = 0.75f;

            [RangeMinimum(1), Description("The time interval of resource audits to maintain the desired state across the fleet of pods.")]
            public int SyncPeriodSeconds { get; set; } = 5;
            [RangeMinimum(0), Description("The stabilization window for scaling up.")]
            public int ScaleUpStabilizationWindowSeconds { get; set; } = 10;
            [RangeMinimum(0), Description("The stabilization window for scaling down.")]
            public int ScaleDownStabilizationWindowSeconds { get; set; } = 300;

            [Required, EmbeddedResource, Description("The pod template of live streaming server.")]
            public V1PodTemplateSpec Template { get; set; } = new();
        }

        public class EntityStatus
        {
            public int ActivePods { get; set; }
            public int PendingStopPods { get; set; }
            public int TotalStreams { get; set; }
        }
    }
}
