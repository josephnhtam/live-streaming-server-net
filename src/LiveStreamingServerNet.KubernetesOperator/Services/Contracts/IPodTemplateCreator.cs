using k8s.Models;
using LiveStreamingServerNet.KubernetesOperator.Entities;

namespace LiveStreamingServerNet.KubernetesOperator.Services.Contracts
{
    public interface IPodTemplateCreator
    {
        V1PodTemplateSpec CreatePodTemplate(V1LiveStreamingServerFleet entity);
    }
}
