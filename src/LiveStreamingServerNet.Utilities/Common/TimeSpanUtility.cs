namespace LiveStreamingServerNet.Utilities.Common
{
    public static class TimeSpanUtility
    {
        public static TimeSpan Min(TimeSpan a, TimeSpan b)
        {
            return a < b ? a : b;
        }

        public static TimeSpan Max(TimeSpan a, TimeSpan b)
        {
            return a > b ? a : b;
        }
    }
}
