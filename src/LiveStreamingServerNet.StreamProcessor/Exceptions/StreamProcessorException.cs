namespace LiveStreamingServerNet.StreamProcessor.Exceptions
{
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
