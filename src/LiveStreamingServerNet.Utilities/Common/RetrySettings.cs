namespace LiveStreamingServerNet.Utilities.Common
{
    public record RetrySettings(int MaxAttempts, TimeSpan InitialBackoff, TimeSpan MaxBackoff, double BackoffMultiplier);
}
