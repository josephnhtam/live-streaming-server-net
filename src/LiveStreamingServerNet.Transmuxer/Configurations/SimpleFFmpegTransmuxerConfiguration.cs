namespace LiveStreamingServerNet.Transmuxer.Configurations
{
    public class SimpleFFmpegTransmuxerConfiguration
    {
        public required string FFmpegPath { get; set; }
        public required string FFmpegTransmuxerArguments { get; set; }
    }
}
