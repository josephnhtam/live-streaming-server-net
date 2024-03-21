namespace LiveStreamingServerNet.Operator.Models
{
    public class ClusterState
    {
        public IReadOnlyList<PodState> Pods { get; }
        public IReadOnlyList<PodState> ActivePods { get; }

        public ClusterState(IReadOnlyList<PodState> pods)
        {
            Pods = pods;
            ActivePods = pods.Where(x => x.Phase <= PodPhase.Running && !x.PendingStop).ToList();
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
