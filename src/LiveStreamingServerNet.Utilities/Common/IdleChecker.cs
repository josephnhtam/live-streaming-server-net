using LiveStreamingServerNet.Utilities.Common.Contracts;

namespace LiveStreamingServerNet.Utilities.Common
{
    public class IdleChecker : IIdleChecker
    {
        private readonly TimeSpan _maxIdleTime;
        private readonly Action _onMaxIdleTimeExceeded;
        private readonly Timer _timer;

        private DateTime _lastRefreshTime;

        public IdleChecker(TimeSpan checkInterval, TimeSpan maxIdleTime, Action onMaxIdleTimeExceeded)
        {
            _lastRefreshTime = DateTime.UtcNow;
            _maxIdleTime = maxIdleTime;
            _onMaxIdleTimeExceeded = onMaxIdleTimeExceeded;
            _timer = new Timer(PerformCheck, null, checkInterval, checkInterval);
        }

        private void PerformCheck(object? state)
        {
            if (DateTime.UtcNow - _lastRefreshTime > _maxIdleTime)
            {
                _onMaxIdleTimeExceeded.Invoke();
                _timer.Dispose();
            }
        }

        public void Refresh()
        {
            _lastRefreshTime = DateTime.UtcNow;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
