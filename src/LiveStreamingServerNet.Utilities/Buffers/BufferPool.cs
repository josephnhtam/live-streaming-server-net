using LiveStreamingServerNet.Utilities.Buffers.Configurations;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Utilities.Buffers
{
    public class BufferPool : IBufferPool
    {
        private readonly BufferPoolConfiguration _config;
        private readonly ArrayPool<byte> _pool;

        public static IBufferPool Shared { get; } = new BufferPool(new BufferPoolConfiguration());

        public BufferPool(IOptions<BufferPoolConfiguration> config) :
            this(config.Value)
        {
        }

        public BufferPool(BufferPoolConfiguration config)
        {
            _config = config;
            _pool = ArrayPool<byte>.Create(_config.MaxBufferSize, _config.MaxBuffersPerBucket);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] Rent(int minimumLength)
        {
            if (minimumLength >= _config.MaxBufferSize)
            {
                minimumLength = MathUtility.NextPowerOfTwo(minimumLength);
            }

            return _pool.Rent(Math.Max(_config.MinBufferSize, minimumLength));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(byte[] buffer)
        {
            _pool.Return(buffer);
        }
    }
}
