namespace LiveStreamingServerNet.Rtmp.Client.Exceptions
{
    public class RtmpClientCommandException : Exception
    {
        public RtmpClientCommandException()
        {
        }

        public RtmpClientCommandException(string? message) : base(message)
        {
        }

        public RtmpClientCommandException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
