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
        public static TReturn? Execute<TReturn>(Func<TReturn?> action, Func<Exception, TReturn?>? onException = null)
            => Execute<TReturn, Exception>(action, onException);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TReturn? Execute<TReturn, TException>(Func<TReturn?> action, Func<TException, TReturn?>? onException = null)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task ExecuteAsync(Task task, Action<Exception>? onException = null)
            => ExecuteAsync<Exception>(task, onException);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task ExecuteAsync<TException>(Task task, Action<TException>? onException = null)
            where TException : Exception
        {
            try
            {
                await task;
            }
            catch (TException ex)
            {
                onException?.Invoke(ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task ExecuteAsync(Func<Task> action, Func<Exception, Task>? onException = null)
            => ExecuteAsync<Exception>(action, onException);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task ExecuteAsync<TException>(Func<Task> action, Func<TException, Task>? onException = null)
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
                    return Task.CompletedTask;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<TReturn?> ExecuteAsync<TReturn>(Func<Task<TReturn?>> action, Func<Exception, Task<TReturn?>>? onException = null)
            => ExecuteAsync<TReturn, Exception>(action, onException);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<TReturn?> ExecuteAsync<TReturn, TException>(Func<Task<TReturn?>> action, Func<TException, Task<TReturn?>>? onException = null)
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
                    return Task.FromResult(default(TReturn));
                }
            }
        }
    }
}
