using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal class TcpListenerWrapper : ITcpListenerInternal
    {
        private readonly TcpListener _tcpListener;
        private readonly NetworkConfiguration _config;

        public TcpListenerWrapper(TcpListener tcpListener, NetworkConfiguration config)
        {
            _tcpListener = tcpListener;
            _config = config;
        }

        public EndPoint LocalEndpoint => _tcpListener.LocalEndpoint;
        public void Start() => _tcpListener.Start();
        public void Stop() => _tcpListener.Stop();
        public bool Pending() => _tcpListener.Pending();

        public async ValueTask<ITcpClientInternal> AcceptTcpClientAsync(CancellationToken cancellationToken)
        {
            var tcpClient = await _tcpListener.AcceptTcpClientAsync(cancellationToken);

            if (_config.PreferInlineCompletionsOnNonWindows && Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                typeof(Socket).GetProperty("PreferInlineCompletions", BindingFlags.NonPublic | BindingFlags.Instance)?
                    .SetValue(tcpClient.Client, true);
            }

            tcpClient.ReceiveBufferSize = _config.ReceiveBufferSize;
            tcpClient.SendBufferSize = _config.SendBufferSize;
            tcpClient.NoDelay = _config.NoDelay;

            return new TcpClientWrapper(tcpClient);
        }
    }
}
