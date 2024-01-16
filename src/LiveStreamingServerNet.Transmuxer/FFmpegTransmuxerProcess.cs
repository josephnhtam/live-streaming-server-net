using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Exceptions;
using System.Diagnostics;

namespace LiveStreamingServerNet.Transmuxer
{
    public class FFmpegTransmuxerProcess : ITransmuxerProcess
    {
        private readonly string _ffmpegPath;
        private readonly string _arguments;

        private const int _gracefulTerminationSeconds = 5;

        public FFmpegTransmuxerProcess(string ffmpegPath, string arguments)
        {
            _ffmpegPath = ffmpegPath;
            _arguments = arguments;
        }

        public async Task RunAsync(string inputPath, string outputPath, CancellationToken cancellation)
        {
            using var process = new Process();

            try
            {
                await RunProcessAsync(inputPath, outputPath, process, cancellation);
            }
            catch (Exception ex)
            {
                await TerminateProcessGracefully(process, _gracefulTerminationSeconds);

                if (ex is OperationCanceledException && cancellation.IsCancellationRequested)
                    throw;

                throw new TransmuxerProcessException("Error running FFmpeg process", ex);
            }
        }

        private async Task RunProcessAsync(string inputPath, string outputPath, Process process, CancellationToken cancellation)
        {
            var arguments = _arguments
                .Replace("{inputPath}", inputPath, StringComparison.InvariantCultureIgnoreCase)
                .Replace("{outputPath}", outputPath, StringComparison.InvariantCultureIgnoreCase);

            process.StartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            if (!process.Start())
                throw new TransmuxerProcessException("Error starting FFmpeg process");

            await process.WaitForExitAsync(cancellation);
        }

        private static async Task TerminateProcessGracefully(Process process, int gracefulPeriod)
        {
            if (!process.HasExited && process.CloseMainWindow())
            {
                using var shutdownCts = new CancellationTokenSource(TimeSpan.FromSeconds(gracefulPeriod));
                try
                {
                    await process.WaitForExitAsync(shutdownCts.Token);
                }
                catch (OperationCanceledException) { }
                catch (Exception shutdownEx)
                {
                    throw new TransmuxerProcessException("Error shutting down FFmpeg process", shutdownEx);
                }
            }

            if (!process.HasExited)
                process.Kill();
        }
    }
}
