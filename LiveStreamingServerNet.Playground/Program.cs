using LiveStreamingServerNet.Builders;
using Serilog;
using System.Net;

namespace LiveStreamingServerNet.Playground
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            using var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            Console.CancelKeyPress += (s, e) =>
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                logger.Information("Stopping...");
                cts.Cancel();
                e.Cancel = true;
            };

            var liveStreamingServer = LiveStreamingServerBuilder.Create()
                .ConfigureLogging(options =>
                {
                    options.AddSerilog(logger);
                })
                .Build();

            var runTask = liveStreamingServer.RunAsync(new IPEndPoint(IPAddress.Any, 1935), cancellationToken);

            try
            {
                await runTask;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        }
    }
}
