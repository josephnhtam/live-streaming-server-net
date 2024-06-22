using LiveStreamingServerNet.Utilities.Common.Contracts;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Utilities.Common
{
    public sealed class Pool<TObject> : IPool<TObject> where TObject : class
    {
        private readonly ConcurrentQueue<TObject> _pool;
        private readonly Func<TObject> _objectFactory;
        private readonly Action<TObject>? _obtainCallback;
        private readonly Action<TObject>? _recycleCallback;

        public Pool(Func<TObject> objectFactory, Action<TObject>? obtainCallback = null, Action<TObject>? recycleCallback = null)
        {
            _pool = new ConcurrentQueue<TObject>();
            _objectFactory = objectFactory;
            _obtainCallback = obtainCallback;
            _recycleCallback = recycleCallback;
        }

        public TObject Obtain()
        {
            TObject obtained;

            if (_pool.TryDequeue(out var obj))
                obtained = obj;
            else
                obtained = _objectFactory();

            _obtainCallback?.Invoke(obtained);
            return obtained;
        }

        public void Recycle(TObject obj)
        {
            _recycleCallback?.Invoke(obj);
            _pool.Enqueue(obj);
        }

        public int GetPooledCount() => _pool.Count;
    }
}
