namespace LiveStreamingServerNet.StreamProcessor.Internal.FFprobe
{
    internal partial class FFprobeProcess
    {
        public record struct Configuration(
           string FFprobePath,
           string Arguments,
           int GracefulTerminationSeconds
       );
    }
}
