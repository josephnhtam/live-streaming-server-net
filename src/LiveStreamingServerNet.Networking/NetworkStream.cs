using LiveStreamingServerNet.Networking.Contracts;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Networking
{
    public class NetworkStream : INetworkStream
    {
        public Stream InnerStream { get; }

        public NetworkStream(Stream stream)
        {
            InnerStream = stream;
        }

        public void Dispose()
        {
            InnerStream.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask ReadExactlyAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            return InnerStream.ReadExactlyAsync(buffer, offset, count, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            return InnerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }
    }
}
