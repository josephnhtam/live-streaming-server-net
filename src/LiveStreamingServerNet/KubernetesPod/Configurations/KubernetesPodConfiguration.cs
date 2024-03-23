namespace LiveStreamingServerNet.KubernetesPod.Configurations
{
    public class KubernetesPodConfiguration
    {
        public bool BlockPublishingWhenPendingStop { get; set; } = true;
        public bool BlockPublishingWhenLimitReached { get; set; } = true;
    }
}
