using LiveStreamingServerNet.Utilities.Buffers.Configurations;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using LiveStreamingServerNet.Utilities.Common;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Utilities.Buffers
{
    public sealed class DataBufferPool : IDataBufferPool
    {
        private readonly Pool<IDataBuffer> _pool;
        private readonly IBufferPool? _bufferPool;
        private readonly int _initialCapacity;
        private readonly int _maxPoolSize;

        public IBufferPool? BufferPool => _bufferPool;

        public DataBufferPool(IOptions<DataBufferPoolConfiguration> config, IBufferPool? bufferPool = null)
        {
            _bufferPool = bufferPool;
            _initialCapacity = config.Value.BufferInitialCapacity;
            _maxPoolSize = config.Value.MaxPoolSize;
            _pool = new Pool<IDataBuffer>(CreateDataBuffer);
        }

        private IDataBuffer CreateDataBuffer()
        {
            return new DataBuffer(_bufferPool, _initialCapacity);
        }

        public IDataBuffer Obtain()
        {
            return _pool.Obtain();
        }

        public void Recycle(IDataBuffer dataBuffer)
        {
            if (_pool.GetPooledCount() >= _maxPoolSize)
            {
                dataBuffer.Dispose();
                return;
            }

            _pool.Recycle(dataBuffer);
        }
    }
}
