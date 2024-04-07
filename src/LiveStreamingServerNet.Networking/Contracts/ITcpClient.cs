using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface ITcpClient
    {
        int Available { get; }
        bool Connected { get; }
        int ReceiveBufferSize { get; set; }
        int SendBufferSize { get; set; }
        int ReceiveTimeout { get; set; }
        int SendTimeout { get; set; }
        bool NoDelay { get; set; }
        [DisallowNull] LingerOption? LingerState { get; set; }
    }
}
