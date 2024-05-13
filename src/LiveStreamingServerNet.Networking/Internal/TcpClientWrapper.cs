using LiveStreamingServerNet.Networking.Internal.Contracts;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal class TcpClientWrapper : ITcpClientInternal
    {
        private readonly TcpClient _tcpClient;

        public TcpClientWrapper(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }

        public int Available => _tcpClient.Available;
        public bool Connected => _tcpClient.Connected;
        public Socket Client => _tcpClient.Client;
        public int ReceiveBufferSize { get => _tcpClient.ReceiveBufferSize; set => _tcpClient.ReceiveBufferSize = value; }
        public int SendBufferSize { get => _tcpClient.SendBufferSize; set => _tcpClient.SendBufferSize = value; }
        public int ReceiveTimeout { get => _tcpClient.ReceiveTimeout; set => _tcpClient.ReceiveTimeout = value; }
        public int SendTimeout { get => _tcpClient.SendTimeout; set => _tcpClient.SendTimeout = value; }
        public bool NoDelay { get => _tcpClient.NoDelay; set => _tcpClient.NoDelay = value; }
        [DisallowNull] public LingerOption? LingerState { get => _tcpClient.LingerState; set => _tcpClient.LingerState = value; }
        public void Close() => _tcpClient.Close();
        public Stream GetStream() => _tcpClient.GetStream();
    }
}
