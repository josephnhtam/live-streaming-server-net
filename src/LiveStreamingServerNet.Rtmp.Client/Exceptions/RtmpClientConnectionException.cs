namespace LiveStreamingServerNet.Rtmp.Client.Exceptions
{
    /// <summary>
    /// Exception thrown when RTMP connection establishment fails.
    /// This includes handshake failures, connection timeouts, and network errors.
    /// </summary>
    public class RtmpClientConnectionException : Exception
    {
        public RtmpClientConnectionException()
        {
        }

        public RtmpClientConnectionException(string? message) : base(message)
        {
        }

        public RtmpClientConnectionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
