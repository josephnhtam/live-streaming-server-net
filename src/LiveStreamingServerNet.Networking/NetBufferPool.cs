using LiveStreamingServerNet.Networking.Configurations;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace LiveStreamingServerNet.Networking
{
    public sealed class NetBufferPool : INetBufferPool
    {
        private readonly Pool<INetBuffer> _pool;
        private readonly IBufferPool? _bufferPool;
        private readonly int _initialCapacity;
        private readonly int _maxPoolSize;
        private int _poolSize;

        public IBufferPool? BufferPool => _bufferPool;

        public NetBufferPool(IOptions<NetBufferPoolConfiguration> config, IBufferPool? bufferPool = null)
        {
            _pool = new Pool<INetBuffer>(CreateNetBuffer);
            _bufferPool = bufferPool;
            _initialCapacity = config.Value.NetBufferCapacity;
            _maxPoolSize = config.Value.MaxPoolSize;
        }

        private INetBuffer CreateNetBuffer()
        {
            var netBuffer = new NetBuffer(_bufferPool, _initialCapacity);
            Interlocked.Increment(ref _poolSize);
            return netBuffer;
        }

        public INetBuffer Obtain()
        {
            if (_maxPoolSize >= 0 && _poolSize >= _maxPoolSize && _pool.GetPooledCount() == 0)
                return new NetBuffer(_bufferPool, _initialCapacity);

            var netBuffer = _pool.Obtain();
            netBuffer.Reset();
            return netBuffer;
        }

        public void Recycle(INetBuffer netBuffer)
        {
            Debug.Assert(netBuffer.UnderlyingBuffer != null);
            _pool.Recycle(netBuffer);
        }
    }
}
