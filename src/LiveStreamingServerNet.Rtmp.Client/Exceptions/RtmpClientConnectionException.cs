namespace LiveStreamingServerNet.Rtmp.Client.Exceptions
{
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
