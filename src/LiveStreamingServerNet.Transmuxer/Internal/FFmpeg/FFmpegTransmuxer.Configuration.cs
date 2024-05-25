namespace LiveStreamingServerNet.Transmuxer.Internal.FFmpeg
{
    internal partial class FFmpegTransmuxer
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
