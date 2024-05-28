namespace LiveStreamingServerNet.StreamProcessor.Internal.FFmpeg
{
    internal partial class FFmpegProcess
    {
        public record struct Configuration(
            Guid ContextIdentifier,
            string Name,
            string FFmpegPath,
            string Arguments,
            int GracefulTerminationSeconds,
            string OutputPath
        );
    }
}
