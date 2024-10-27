namespace LiveStreamingServerNet.StreamProcessor.Exceptions
{
    /// <summary>
    /// Exception thrown when stream processing operations fail.
    /// </summary>
    public class StreamProcessorException : Exception
    {
        public StreamProcessorException()
        {
        }

        public StreamProcessorException(string? message) : base(message)
        {
        }

        public StreamProcessorException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
