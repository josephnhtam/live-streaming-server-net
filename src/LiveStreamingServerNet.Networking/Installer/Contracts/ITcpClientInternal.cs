using LiveStreamingServerNet.Networking.Contracts;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Networking.Installer.Contracts
{
    internal interface ITcpClientInternal : ITcpClient
    {
        void Close();
        Socket Client { get; }
        Stream GetStream();
    }
}
