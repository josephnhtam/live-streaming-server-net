using LiveStreamingServerNet.Networking.Installer.Contracts;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using System.Net;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal class TcpListenerWrapper : ITcpListenerInternal
    {
        private readonly TcpListener _tcpListener;

        public TcpListenerWrapper(TcpListener tcpListener)
        {
            _tcpListener = tcpListener;
        }

        public EndPoint LocalEndpoint => _tcpListener.LocalEndpoint;
        public void Start() => _tcpListener.Start();
        public void Stop() => _tcpListener.Stop();
        public bool Pending() => _tcpListener.Pending();

        public async ValueTask<ITcpClientInternal> AcceptTcpClientAsync(CancellationToken cancellationToken)
        {
            var tcpClient = await _tcpListener.AcceptTcpClientAsync(cancellationToken);
            return new TcpClientWrapper(tcpClient);
        }
    }
}
