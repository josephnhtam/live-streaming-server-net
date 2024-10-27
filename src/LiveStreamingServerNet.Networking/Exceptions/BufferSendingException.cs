namespace LiveStreamingServerNet.Networking.Exceptions
{
    /// <summary>
    /// Exception thrown when buffer sending operation fails.
    /// </summary>
    public class BufferSendingException : Exception
    {
        public BufferSendingException() : base() { }
        private BufferSendingException(string? message) : base(message) { }
        private BufferSendingException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
