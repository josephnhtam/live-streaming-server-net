using System.Buffers;
using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Utilities
{
    public static class BufferPool
    {
        private const int _maxBufferLength = 16 * 1024 * 1024;
        private const int _maxNumberOfBuffersPerBucket = 50;

        private static ArrayPool<byte> _pool;

        static BufferPool()
        {
            _pool = ArrayPool<byte>.Create(
                _maxBufferLength,
                Math.Max(1, Environment.ProcessorCount) * _maxNumberOfBuffersPerBucket
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Rent(int minimumLength)
        {
            return _pool.Rent(minimumLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(byte[] buffer)
        {
            _pool.Return(buffer);
        }
    }
}
