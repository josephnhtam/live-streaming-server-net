using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Rtmp.Core.Contracts;

namespace LiveStreamingServer.Rtmp.Core
{
    public class FixedNetBuffer : NetBuffer, IFixedNetBuffer
    {
        private byte[] _buffer;

        private FixedNetBuffer(byte[] buffer) : base(buffer)
        {
            _buffer = buffer;
        }

        public static FixedNetBuffer Create(int capacity)
        {
            return new FixedNetBuffer(new byte[capacity]);
        }

        public async Task ReadExactlyAsync(Stream stream, int bytesCount, CancellationToken cancellationToken)
        {
            await stream.ReadExactlyAsync(_buffer, 0, bytesCount, cancellationToken);
            Position = 0;
        }
    }
}
