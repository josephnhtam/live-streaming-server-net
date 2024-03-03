using LiveStreamingServerNet.Utilities.Contracts;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Utilities
{
    public sealed class Pool<TObject> : IPool<TObject> where TObject : class
    {
        private readonly ConcurrentQueue<TObject> _pool;
        private readonly Func<TObject> _objectFactory;
        private readonly Action<TObject>? _recycleCallback;

        public Pool(Func<TObject> objectFactory, Action<TObject>? recycleCallback = null)
        {
            _pool = new ConcurrentQueue<TObject>();
            _objectFactory = objectFactory;
            _recycleCallback = recycleCallback;
        }

        public TObject Obtain()
        {
            if (_pool.TryDequeue(out var obj))
                return obj;

            return _objectFactory();
        }

        public void Recycle(TObject obj)
        {
            _recycleCallback?.Invoke(obj);
            _pool.Enqueue(obj);
        }

        public int GetPooledCount() => _pool.Count;
    }
}
