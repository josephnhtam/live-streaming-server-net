using LiveStreamingServerNet.Utilities.Common.Contracts;

namespace LiveStreamingServerNet.Utilities.Common
{
    public class RetryCounter : IRetryCounter
    {
        private readonly RetrySettings _settings;
        private int _attempts;

        public RetryCounter(RetrySettings settings)
        {
            _settings = settings;
            _attempts = 0;
        }

        public void Reset()
        {
            _attempts = 0;
        }

        public bool CanRetry()
        {
            return _attempts < _settings.MaxAttempts;
        }

        public TimeSpan? GetNextBackoff()
        {
            if (!CanRetry())
                return null;

            var backoff = _settings.InitialBackoff.TotalMilliseconds * Math.Pow(_settings.BackoffMultiplier, _attempts);
            backoff = Math.Min(backoff, _settings.MaxBackoff.TotalMilliseconds);

            _attempts++;
            return TimeSpan.FromMilliseconds(backoff);
        }
    }
}
