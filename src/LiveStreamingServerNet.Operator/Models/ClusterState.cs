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

    public record PodState(string PodName, bool PendingStop, int StreamsCount, PodPhase Phase, DateTime? StartTime);

    public enum PodPhase
    {
        Unknown = -1,
        Pending = 0,
        Running = 1,
        Terminating = 3,
        Succeeded = 4,
        Failed = 5
    }
}
