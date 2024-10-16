namespace LiveStreamingServerNet.Utilities.Extensions
{
    public static class TaskExtensions
    {
        public static async Task WithCancellation(this Task task, CancellationToken cancellation)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            try
            {
                var completedTask = await Task.WhenAny(task, Task.Delay(Timeout.Infinite, cts.Token));
                await completedTask;
            }
            finally
            {
                cts.Cancel();
            }
        }
    }
}
