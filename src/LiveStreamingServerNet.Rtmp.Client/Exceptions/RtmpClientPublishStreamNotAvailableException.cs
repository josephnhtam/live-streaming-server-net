namespace LiveStreamingServerNet.Rtmp.Client.Exceptions
{
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
