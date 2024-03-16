namespace LiveStreamingServerNet.KubernetesPod.Configurations
{
    public class KubernetesPodConfiguration
    {
        public string PodNamespaceEnvironmentVariableName { get; set; } = "POD_NAMESPACE";
        public string PodNameEnvironmentVariableName { get; set; } = "POD_NAME";
    }
}
