using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Utilities
{
    public class Rentable<T> : IRentable<T>
    {
        private readonly Action? _callback;

        public T Value { get; }

        public Rentable(T value, Action? callback)
        {
            Value = value;
            _callback = callback;
        }

        public void Dispose()
        {
            _callback?.Invoke();
        }
    }
}
