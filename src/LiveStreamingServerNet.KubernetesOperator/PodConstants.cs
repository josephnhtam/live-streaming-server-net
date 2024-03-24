namespace LiveStreamingServerNet.KubernetesOperator
{
    internal static class PodConstants
    {
        public const string TypeLabel = "live-streaming-server-net/type";
        public const string TypeValue = "live-streaming-server-pod";

        public const string PendingStopLabel = "live-streaming-server-net/pending-stop";
        public const string StreamsLimitReachedLabel = "live-streaming-server-net/streams-limit-reached";

        public const string StreamsCountAnnotation = "live-streaming-server-net/streams-count";
        public const string StreamsLimitAnnotation = "live-streaming-server-net/streams-limit";

        public const string StreamsLimitEnv = "STREAMS_LIMIT";
    }
}
