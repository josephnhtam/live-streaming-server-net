namespace LiveStreamingServerNet.StreamProcessor.FFmpeg.Exceptions
{
    public class FFmpegProcessException : Exception
    {
        public int ExitCode { get; }

        public FFmpegProcessException(int exitCode) : base($"FFmpeg process exited with code {exitCode}")
        {
            ExitCode = exitCode;
        }

        public FFmpegProcessException(string message) : base(message)
        {
            ExitCode = -1;
        }
    }
}
