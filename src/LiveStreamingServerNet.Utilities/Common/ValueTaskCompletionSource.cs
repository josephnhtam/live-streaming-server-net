using System.Threading.Tasks.Sources;

namespace LiveStreamingServerNet.Utilities.Common
{
    public class ValueTaskCompletionSource : IValueTaskSource
    {
        private ManualResetValueTaskSourceCore<bool> _valueTaskSource;

        void IValueTaskSource.GetResult(short token)
        {
            _valueTaskSource.GetResult(token);
        }

        ValueTaskSourceStatus IValueTaskSource.GetStatus(short token)
        {
            return _valueTaskSource.GetStatus(token);
        }

        void IValueTaskSource.OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            _valueTaskSource.OnCompleted(continuation, state, token, flags);
        }

        public void SetResult()
        {
            _valueTaskSource.SetResult(true);
        }

        public void SetException(Exception exception)
        {
            _valueTaskSource.SetException(exception);
        }

        public ValueTask Task => new ValueTask(this, _valueTaskSource.Version);
    }
}
