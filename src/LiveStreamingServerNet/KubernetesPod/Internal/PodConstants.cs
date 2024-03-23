﻿namespace LiveStreamingServerNet.KubernetesPod.Internal
{
    internal static class PodConstants
    {
        public const string PendingStopLabel = "live-streaming-server-net/pending-stop";
        public const string LimitReachedLabel = "live-streaming-server-net/limit-reached";

        public const string StreamsCountAnnotation = "live-streaming-server-net/streams-count";
        public const string StreamsLimitAnnotation = "live-streaming-server-net/streams-limit";

        public const string StreamsLimitEnv = "STREAMS_LIMIT";
    }
}