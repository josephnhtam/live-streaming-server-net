namespace LiveStreamingServerNet.Rtmp.Client.Exceptions
{
    public class RtmpStreamDeletedException : Exception
    {
        public RtmpStreamDeletedException()
        {
        }

        public RtmpStreamDeletedException(string? message) : base(message)
        {
        }

        public RtmpStreamDeletedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
