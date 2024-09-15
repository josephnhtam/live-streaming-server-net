using LiveStreamingServerNet.Networking.Client.Internal.Contracts;
using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Networking.Internal;
using LiveStreamingServerNet.Networking.Internal.Contracts;
using Microsoft.Extensions.Options;
using System.Net.Sockets;
using System.Reflection;

namespace LiveStreamingServerNet.Networking.Client.Internal
{
    internal class TcpClientFactory : ITcpClientFactory
    {
        private readonly NetworkConfiguration _config;

        public TcpClientFactory(IOptions<NetworkConfiguration> config)
        {
            _config = config.Value;
        }

        public ITcpClientInternal Create()
        {
            var tcpClient = new TcpClient();

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
