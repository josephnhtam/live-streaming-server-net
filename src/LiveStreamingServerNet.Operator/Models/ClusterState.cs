namespace LiveStreamingServerNet.Operator.Models
{
    public class ClusterState
    {
        public int PodsCount => PodStates.Count;
        public IReadOnlyList<PodState> PodStates { get; }

        public ClusterState(IReadOnlyList<PodState> podStates)
        {
            PodStates = podStates;
        }
    }

    public record PodState(string PodName, bool PendingStop, int StreamsCount, DateTime? StartTime);
}
