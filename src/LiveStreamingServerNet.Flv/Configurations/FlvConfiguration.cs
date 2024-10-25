namespace LiveStreamingServerNet.Flv.Configurations
{
    public class FlvConfiguration
    {
        public TimeSpan ReadinessTimeout { get; set; } = TimeSpan.FromSeconds(15);

        public TimeSpan StreamContinuationTimeout { get; set; } = TimeSpan.Zero;
    }
}
