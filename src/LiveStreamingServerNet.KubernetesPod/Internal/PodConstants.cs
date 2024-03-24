namespace LiveStreamingServerNet.KubernetesPod.Internal
{
    internal static class PodConstants
    {
        public const string PendingStopLabel = "live-streaming-server-net/pending-stop";
        public const string StreamsLimitReachedLabel = "live-streaming-server-net/streams-limit-reached";

        public const string StreamsCountAnnotation = "live-streaming-server-net/streams-count";
        public const string StreamsLimitAnnotation = "live-streaming-server-net/streams-limit";

        public const string StreamsLimitEnv = "STREAMS_LIMIT";
        public const string PodNamespaceEnv = "POD_NAMESPACE";
        public const string PodNameEnv = "POD_NAME";
    }
}
