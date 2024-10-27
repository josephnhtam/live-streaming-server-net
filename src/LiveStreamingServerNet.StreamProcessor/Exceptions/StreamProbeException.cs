namespace LiveStreamingServerNet.StreamProcessor.Exceptions
{
    /// <summary>
    /// Exception thrown when stream probing operations fail.
    /// Stream probing typically involves analyzing stream characteristics and metadata.
    /// </summary>
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
