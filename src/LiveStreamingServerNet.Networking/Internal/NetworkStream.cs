using LiveStreamingServerNet.Networking.Contracts;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Networking.Internal
{
    internal class NetworkStream : INetworkStream
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
            return InnerStream.ReadExactlyAsync(new Memory<byte>(buffer, offset, count), cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            return InnerStream.WriteAsync(new Memory<byte>(buffer, offset, count), cancellationToken);
        }
    }
}
