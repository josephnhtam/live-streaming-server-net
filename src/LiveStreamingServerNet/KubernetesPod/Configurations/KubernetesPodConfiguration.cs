namespace LiveStreamingServerNet.KubernetesPod.Configurations
{
    public class KubernetesPodConfiguration
    {
        public string PodNamespaceEnvironmentVariableName { get; set; } = "POD_NAMESPACE";
        public string PodNameEnvironmentVariableName { get; set; } = "POD_NAME";
        public string LabelPendingStop { get; set; } = "live-streaming-server-net/pending-stop";
        public string AnnotationStreamCount { get; set; } = "stream-count";
    }
}
