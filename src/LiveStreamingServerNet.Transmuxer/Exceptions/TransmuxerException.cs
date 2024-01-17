namespace LiveStreamingServerNet.Transmuxer.Exceptions
{
    public class TransmuxerException : Exception
    {
        public TransmuxerException()
        {
        }

        public TransmuxerException(string? message) : base(message)
        {
        }

        public TransmuxerException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
