namespace LiveStreamingServerNet.Operator.Models
{
    public class DesiredClusterStateChange
    {
        public uint PodsIncrement { get; }
        public IReadOnlyList<PodStateChange> PodStateChanges { get; }

        public DesiredClusterStateChange(uint podsIncrement, IReadOnlyList<PodStateChange> podStateChanges)
        {
            PodsIncrement = podsIncrement;
            PodStateChanges = podStateChanges;
        }
    }

    public record PodStateChange(string PodName, bool PendingStop);
}
