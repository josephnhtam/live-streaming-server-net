using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Exceptions;
using System.Diagnostics;

namespace LiveStreamingServerNet.Transmuxer
{
    public class FFmpegTransmuxer : ITransmuxer
    {
        private readonly string _identifier;
        private readonly string _ffmpegPath;
        private readonly string _arguments;
        private readonly string _outputFileName;
        private readonly int _gracefulTerminationSeconds;

        public FFmpegTransmuxer(string identifier, string ffmpegPath, string arguments, string outputFileName, int gracefulTerminationSeconds)
        {
            _identifier = identifier;
            _ffmpegPath = ffmpegPath;
            _arguments = arguments;
            _outputFileName = outputFileName;
            _gracefulTerminationSeconds = gracefulTerminationSeconds;
        }

        public async Task RunAsync(
            string inputPath,
            string outputDirPath,
            OnTransmuxerStarted? onStarted,
            OnTransmuxerEnded? onEnded,
            CancellationToken cancellation)
        {
            new DirectoryInfo(outputDirPath).Create();
            var outputPath = Path.Combine(outputDirPath, _outputFileName);

            await RunProcessAsync(inputPath, outputPath, onStarted, onEnded, cancellation);
        }

        private async Task RunProcessAsync(string inputPath, string outputPath, OnTransmuxerStarted? onStarted, OnTransmuxerEnded? onEnded, CancellationToken cancellation)
        {
            var arguments = _arguments
                .Replace("{inputPath}", inputPath, StringComparison.InvariantCultureIgnoreCase)
                .Replace("{outputPath}", outputPath, StringComparison.InvariantCultureIgnoreCase);

            using var process = new Process();

            try
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                };

                if (!process.Start())
                    throw new TransmuxerException("Error starting FFmpeg process");

                if (onStarted != null)
                    await onStarted.Invoke(_identifier, outputPath);

                await process.WaitForExitAsync(cancellation);

                if (process.ExitCode != 0)
                    throw new TransmuxerException($"FFmpeg process exited with code {process.ExitCode}");
            }
            catch (Exception ex)
            {
                await TerminateProcessGracefully(process, _gracefulTerminationSeconds);

                if (ex is OperationCanceledException && cancellation.IsCancellationRequested)
                    throw;

                throw new TransmuxerException("Error running FFmpeg process", ex);
            }
            finally
            {
                if (onEnded != null)
                    await onEnded.Invoke(_identifier, outputPath);
            }
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
                    throw new TransmuxerException("Error shutting down FFmpeg process", shutdownEx);
                }
            }

            if (!process.HasExited)
                process.Kill();
        }
    }
}
