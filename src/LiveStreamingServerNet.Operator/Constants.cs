namespace LiveStreamingServerNet.Operator
{
    internal static class Constants
    {
        public const string StreamingServerPodLabelSelector = "app=live-streaming-server.net/app,server-type=streaming-server";
        public const string PendingStopLabel = "live-streaming-server-net/pending-stop";
        public const string StreamsCountAnnotation = "streams-count";
    }
}
