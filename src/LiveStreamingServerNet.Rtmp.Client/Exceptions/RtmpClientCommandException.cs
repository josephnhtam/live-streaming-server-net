namespace LiveStreamingServerNet.Rtmp.Client.Exceptions
{
    /// <summary>
    /// Exception thrown when an RTMP command fails to be sent to the server.
    /// This can occur due to network issues, disconnection, or invalid command state.
    /// </summary>
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
