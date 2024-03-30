namespace LiveStreamingServerNet.KubernetesPod.Redis.Configurations
{
    public class RedisStoreConfiguration
    {
        public string StreamKeyPrefix { get; set; } = "stream:";
    }
}
