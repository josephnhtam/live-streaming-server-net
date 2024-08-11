using LiveStreamingServerNet.Networking.Internal;

namespace LiveStreamingServerNet.Networking.Server.Internal
{
    internal class ClientNetworkStream : NetworkStream
    {
        private readonly uint _clientId;

        public ClientNetworkStream(uint clientId, Stream stream) : base(stream)
        {
            _clientId = clientId;
        }

        public override string ToString()
        {
            return $"NetworkStream (ClientId={_clientId})";
        }
    }
}
