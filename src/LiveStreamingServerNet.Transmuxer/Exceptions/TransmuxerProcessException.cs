namespace LiveStreamingServerNet.Transmuxer.Exceptions
{
    public class TransmuxerProcessException : Exception
    {
        public TransmuxerProcessException()
        {
        }

        public TransmuxerProcessException(string? message) : base(message)
        {
        }

        public TransmuxerProcessException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
