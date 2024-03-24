using k8s.Models;
using LiveStreamingServerNet.KubernetesOperator.Entities;
using LiveStreamingServerNet.KubernetesOperator.Services.Contracts;

namespace LiveStreamingServerNet.KubernetesOperator.Services
{
    public class PodTemplateCreator : IPodTemplateCreator
    {
        public V1PodTemplateSpec CreatePodTemplate(V1LiveStreamingServerFleet entity)
        {
            var template = entity.Spec.Template;

            template.Metadata.Labels ??= new Dictionary<string, string>();
            template.Metadata.Labels[PodConstants.TypeLabel] = PodConstants.TypeValue;
            template.Metadata.Labels[PodConstants.PendingStopLabel] = "false";
            template.Metadata.Labels[PodConstants.StreamsLimitReachedLabel] = "false";

            template.Metadata.Annotations ??= new Dictionary<string, string>();
            template.Metadata.Annotations[PodConstants.StreamsCountAnnotation] = "0";
            template.Metadata.Annotations[PodConstants.StreamsLimitAnnotation] = entity.Spec.PodStreamsLimit.ToString();

            template.Spec.RestartPolicy = "Never";

            foreach (var container in template.Spec.Containers)
            {
                container.Env ??= new List<V1EnvVar>();
                container.Env.Add(new V1EnvVar(PodConstants.StreamsLimitEnv, entity.Spec.PodStreamsLimit.ToString()));
            }

            return template;
        }
    }
}
