namespace LiveStreamingServerNet.Networking.Configurations
{
    public class NetworkConfiguration
    {
        public bool PreferInlineCompletionsOnNonWindows { get; set; } = true;
        public int ReceiveBufferSize { get; set; } = 1024 * 1024;
        public int SendBufferSize { get; set; } = 1024 * 1024;
        public bool NoDelay { get; set; } = false;
    }
}
