namespace LiveStreamingServerNet.Utilities.Extensions
{
    public static class LockExtensions
    {
        public static void Lock(this object obj, Action action)
        {
            lock (obj)
            {
                action.Invoke();
            }
        }
    }
}
