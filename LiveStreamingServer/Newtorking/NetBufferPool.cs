using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Utilities;

namespace LiveStreamingServer.Newtorking
{
    public class NetBufferPool : INetBufferPool
    {
        private readonly int _netBufferCapacity;
        private IPool<INetBuffer> _pool;

        public NetBufferPool(int netBufferCapacity = 512)
        {
            _pool = new Pool<INetBuffer>(CreateNetBuffer);
            _netBufferCapacity = netBufferCapacity;
        }

        private INetBuffer CreateNetBuffer()
        {
            return new NetBuffer(_netBufferCapacity);
        }

        public INetBuffer ObtainNetBuffer()
        {
            return _pool.Obtain();
        }

        public void RecycleNetBuffer(INetBuffer netBuffer)
        {
            netBuffer.Reset();
            _pool.Recycle(netBuffer);
        }
    }
}
