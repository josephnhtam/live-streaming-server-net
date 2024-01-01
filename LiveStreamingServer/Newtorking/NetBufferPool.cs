using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Utilities;
using System.Collections.Concurrent;

namespace LiveStreamingServer.Newtorking
{
    public class NetBufferPool : INetBufferPool
    {
        private readonly IPool<PoolableNetBuffer> _pool;
        private readonly int _netBufferCapacity;
        private readonly int _maxPoolSize;
        private ConcurrentBag<PoolableNetBuffer> _poolableNetBuffers = new();

        public NetBufferPool(int netBufferCapacity = 512, int maxPoolSize = 128)
        {
            _pool = new Pool<PoolableNetBuffer>(CreatePoolableNetBuffer);
            _netBufferCapacity = netBufferCapacity;
            _maxPoolSize = maxPoolSize;
        }

        private PoolableNetBuffer CreatePoolableNetBuffer()
        {
            var netBuffer = new PoolableNetBuffer(this, _netBufferCapacity);
            _poolableNetBuffers.Add(netBuffer);
            return netBuffer;
        }

        public INetBuffer Obtain()
        {
            if (_poolableNetBuffers.Count >= _maxPoolSize && _pool.GetPooledCount() == 0)
            {
                return new NetBuffer(_netBufferCapacity);
            }
            else
            {
                var netBuffer = _pool.Obtain();
                netBuffer.OnObtained();
                return netBuffer;
            }
        }

        internal void RecycleNetBuffer(PoolableNetBuffer netBuffer)
        {
            _pool.Recycle(netBuffer);
        }

        public void Dispose()
        {
            foreach (var netBuffer in _poolableNetBuffers)
            {
                netBuffer.Destroy();
            }
        }
    }

    internal class PoolableNetBuffer : NetBuffer
    {
        private readonly NetBufferPool _pool;
        private int _inPool;

        public PoolableNetBuffer(NetBufferPool pool, int initialCapacity) : base(initialCapacity)
        {
            _pool = pool;
            _inPool = 1;
        }

        public void OnObtained()
        {
            _inPool = 0;
        }

        public override void Dispose()
        {
            if (Interlocked.CompareExchange(ref _inPool, 1, 0) == 1)
            {
                return;
            }

            Reset();
            _pool.RecycleNetBuffer(this);
        }

        public void Destroy()
        {
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        ~PoolableNetBuffer()
        {
            Destroy();
        }
    }
}
