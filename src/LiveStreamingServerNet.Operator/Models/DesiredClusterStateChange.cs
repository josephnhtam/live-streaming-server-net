namespace LiveStreamingServerNet.Operator.Models
{
    public class DesiredClusterStateChange
    {
        public int PodsCountDelta { get; }
        public IReadOnlyList<PodStateChange> PodStateChanges { get; }

        public DesiredClusterStateChange(int podCountDelta, IReadOnlyList<PodStateChange> podStateChanges)
        {
            PodsCountDelta = podCountDelta;
            PodStateChanges = podStateChanges;
        }
    }

    public record PodStateChange(string PodName, bool PendingStop);
}
