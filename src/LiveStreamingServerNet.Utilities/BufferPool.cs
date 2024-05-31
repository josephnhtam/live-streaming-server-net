using LiveStreamingServerNet.Utilities.Configurations;
using LiveStreamingServerNet.Utilities.Contracts;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Utilities
{
    public class BufferPool : IBufferPool
    {
        private readonly BufferPoolConfiguration _config;
        private readonly ArrayPool<byte> _pool;

        public BufferPool(IOptions<BufferPoolConfiguration> config)
        {
            _config = config.Value;
            _pool = ArrayPool<byte>.Create(_config.MaxBufferSize, _config.MaxBuffersPerBucket);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] Rent(int minimumLength)
        {
            return _pool.Rent(Math.Max(_config.MinBufferSize, minimumLength));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(byte[] buffer)
        {
            _pool.Return(buffer);
        }
    }
}
