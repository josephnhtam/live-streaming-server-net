using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Utilities.Common
{
    public static class ErrorBoundary
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Execute(Action action, Action<Exception>? onException = null)
            => Execute<Exception>(action, onException);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Execute<TException>(Action action, Action<TException>? onException = null)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException ex)
            {
                onException?.Invoke(ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TReturn? Execute<TReturn>(Func<TReturn?> action, Func<Exception, TReturn>? onException = null)
            => Execute<TReturn, Exception>(action, onException);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TReturn? Execute<TReturn, TException>(Func<TReturn?> action, Func<TException, TReturn>? onException = null)
            where TException : Exception
        {
            try
            {
                return action();
            }
            catch (TException ex)
            {
                if (onException != null)
                {
                    return onException(ex);
                }
                else
                {
                    return default;
                }
            }
        }
    }
}
