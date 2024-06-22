using LiveStreamingServerNet.Utilities.Common;
using LiveStreamingServerNet.Utilities.Common.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Utilities
{
    public class EventContext : IEventContext, IDisposable
    {
        private static IPool<EventContext> Pool { get; }

        private readonly Dictionary<string, object?> _items;
        private readonly Dictionary<Type, object?> _features;

        private bool _isRecycled;

        public IDictionary<string, object?> Items => _items;

        static EventContext()
        {
            Pool = new Pool<EventContext>(
                () => new EventContext(),
                (obtained) => obtained._isRecycled = false
            );
        }

        public static EventContext Obtain()
        {
            return Pool.Obtain();
        }

        private EventContext()
        {
            _items = new Dictionary<string, object?>();
            _features = new Dictionary<Type, object?>();
        }

        public TFeature? Get<TFeature>() where TFeature : class
        {
            return _features.GetValueOrDefault(typeof(TFeature)) as TFeature;
        }

        public void Set<TFeature>(TFeature instance) where TFeature : class
        {
            _features[typeof(TFeature)] = instance;
        }

        public void Dispose()
        {
            if (_isRecycled)
                return;

            _isRecycled = true;
            _items.Clear();
            _features.Clear();

            Pool.Recycle(this);
        }
    }
}
