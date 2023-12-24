namespace LiveStreamingServer.Utilities
{
    public static class Extensions
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
