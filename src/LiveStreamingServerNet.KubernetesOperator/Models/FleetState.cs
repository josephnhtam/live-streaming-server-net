namespace LiveStreamingServerNet.KubernetesOperator.Models
{
    public class FleetState
    {
        public IReadOnlyList<PodState> Pods { get; }
        public IReadOnlyList<PodState> ActivePods { get; }
        public IReadOnlyList<PodState> PendingStopPods { get; }
        public int TotalStreams { get; }

        public FleetState(IReadOnlyList<PodState> pods)
        {
            Pods = pods;
            ActivePods = pods.Where(x => x.Phase <= PodPhase.Running && !x.PendingStop).ToList();
            PendingStopPods = pods.Where(x => x.Phase <= PodPhase.Running && x.PendingStop).ToList();
            TotalStreams = pods.Where(x => x.Phase <= PodPhase.Running).Sum(x => x.StreamsCount);
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
