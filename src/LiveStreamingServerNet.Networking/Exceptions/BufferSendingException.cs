namespace LiveStreamingServerNet.Networking.Exceptions
{
    public class BufferSendingException : Exception
    {
        public BufferSendingException() : base() { }
        private BufferSendingException(string? message) : base(message) { }
        private BufferSendingException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
