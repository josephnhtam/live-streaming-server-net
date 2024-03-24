namespace LiveStreamingServerNet.KubernetesOperator.Models
{
    public class DesiredFleetStateChange
    {
        public uint PodsIncrement { get; }
        public IReadOnlyList<PodStateChange> PodStateChanges { get; }

        public DesiredFleetStateChange(uint podsIncrement, IReadOnlyList<PodStateChange> podStateChanges)
        {
            PodsIncrement = podsIncrement;
            PodStateChanges = podStateChanges;
        }
    }

    public record PodStateChange(string PodName, bool PendingStop);
}
