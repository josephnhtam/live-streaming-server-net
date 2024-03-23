using k8s.Models;
using KubeOps.Abstractions.Entities;
using KubeOps.Abstractions.Entities.Attributes;

namespace LiveStreamingServerNet.Operator.Entities
{
    [KubernetesEntity(Group = "live-streaming-server.net", ApiVersion = "v1", Kind = "Cluster", PluralName = "Clusters")]
    public partial class V1LiveStreamingServerCluster : CustomKubernetesEntity<V1LiveStreamingServerCluster.EntitySpec, V1LiveStreamingServerCluster.EntityStatus>
    {
        public class EntitySpec
        {
            public int MinReplicas { get; set; } = 1;
            public int MaxReplicas { get; set; } = 10;

            public int PodStreamsLimit { get; set; } = 4;
            public float TargetUtilization { get; set; } = 0.75f;

            public int SyncPeriodSecond { get; set; } = 5;
            public int ScaleUpStabilizationWindowSeconds { get; set; } = 10;
            public int ScaleDownStabilizationWindowSeconds { get; set; } = 300;

            [Required]
            [EmbeddedResource]
            public V1PodTemplateSpec Template { get; set; } = new();
        }

        public class EntityStatus
        {

        }
    }
}
