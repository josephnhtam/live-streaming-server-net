using System.Net.Sockets;

namespace LiveStreamingServerNet.WebRTC.Utilities
{
    public static class SocketUtility
    {
        public static bool IsFatal(this SocketError error)
        {
            return error switch
            {
                SocketError.Shutdown => true,
                SocketError.ConnectionAborted => true,
                SocketError.ConnectionReset => true,
                SocketError.NotConnected => true,
                SocketError.OperationAborted => true,
                SocketError.Interrupted => true,
                _ => false
            };
        }
    }
}
