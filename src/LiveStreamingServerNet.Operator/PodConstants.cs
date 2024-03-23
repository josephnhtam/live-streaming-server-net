namespace LiveStreamingServerNet.Operator
{
    internal static class PodConstants
    {
        public const string AppLabel = "app";
        public const string AppLabelValue = "live-streaming-server-net";

        public const string PendingStopLabel = "live-streaming-server-net/pending-stop";
        public const string StreamsLimitReachedLabel = "live-streaming-server-net/streams-limit-reached";

        public const string StreamsCountAnnotation = "live-streaming-server-net/streams-count";
        public const string StreamsLimitAnnotation = "live-streaming-server-net/streams-limit";

        public const string StreamsLimitEnv = "STREAMS_LIMIT";
    }
}
