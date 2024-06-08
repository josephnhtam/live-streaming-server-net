namespace LiveStreamingServerNet.StreamProcessor.Exceptions
{
    public class StreamProbeException : Exception
    {
        public StreamProbeException()
        {
        }

        public StreamProbeException(string? message) : base(message)
        {
        }

        public StreamProbeException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
