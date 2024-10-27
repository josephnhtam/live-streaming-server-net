namespace LiveStreamingServerNet.Rtmp.Client.Exceptions
{
    /// <summary>
    /// Exception thrown when attempting to publish to a stream that is not available.
    /// This can occur when the stream is not published or has been closed.
    /// </summary>
    public class RtmpClientPublishStreamNotAvailableException : Exception
    {
        public RtmpClientPublishStreamNotAvailableException()
        {
        }

        public RtmpClientPublishStreamNotAvailableException(string? message) : base(message)
        {
        }

        public RtmpClientPublishStreamNotAvailableException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
