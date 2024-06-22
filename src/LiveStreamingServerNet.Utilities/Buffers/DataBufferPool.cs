using LiveStreamingServerNet.Utilities.Buffers.Configurations;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace LiveStreamingServerNet.Utilities.Buffers
{
    public sealed class DataBufferPool : IDataBufferPool
    {
        private readonly Pool<IDataBuffer> _pool;
        private readonly IBufferPool? _bufferPool;
        private readonly int _initialCapacity;
        private readonly int _maxPoolSize;
        private int _poolSize;

        public IBufferPool? BufferPool => _bufferPool;

        public DataBufferPool(IOptions<DataBufferPoolConfiguration> config, IBufferPool? bufferPool = null)
        {
            _pool = new Pool<IDataBuffer>(CreateDataBuffer);
            _bufferPool = bufferPool;
            _initialCapacity = config.Value.BufferInitialCapacity;
            _maxPoolSize = config.Value.MaxPoolSize;
        }

        private IDataBuffer CreateDataBuffer()
        {
            var dataBuffer = new DataBuffer(_bufferPool, _initialCapacity);
            Interlocked.Increment(ref _poolSize);
            return dataBuffer;
        }

        public IDataBuffer Obtain()
        {
            if (_maxPoolSize >= 0 && _poolSize >= _maxPoolSize && _pool.GetPooledCount() == 0)
                return new DataBuffer(_bufferPool, _initialCapacity);

            var dataBuffer = _pool.Obtain();
            dataBuffer.Reset();
            return dataBuffer;
        }

        public void Recycle(IDataBuffer dataBuffer)
        {
            Debug.Assert(dataBuffer.UnderlyingBuffer != null);
            _pool.Recycle(dataBuffer);
        }
    }
}
