using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Rtmp.Test.Utilities
{
    public class NetworkStream : INetworkStream
    {
        public Stream InnerStream { get; }

        public NetworkStream(Stream stream)
        {
            InnerStream = stream;
        }

        public ValueTask ReadExactlyAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            return InnerStream.ReadExactlyAsync(new Memory<byte>(buffer, offset, count), cancellationToken);
        }

        public ValueTask WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            return InnerStream.WriteAsync(new Memory<byte>(buffer, offset, count), cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            return InnerStream.DisposeAsync();
        }
    }
}
