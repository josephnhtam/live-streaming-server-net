using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Exceptions;
using System.Diagnostics;

namespace LiveStreamingServerNet.Transmuxer
{
    public class FFmpegTransmuxer : ITransmuxer
    {
        private readonly string _ffmpegPath;
        private readonly string _arguments;
        private readonly string _outputFileName;
        private readonly bool _createWindow;
        private readonly int _gracefulTerminationSeconds;

        public FFmpegTransmuxer(string ffmpegPath, string arguments, string outputFileName, bool createWindow, int gracefulTerminationSeconds)
        {
            _ffmpegPath = ffmpegPath;
            _arguments = arguments;
            _outputFileName = outputFileName;
            _createWindow = createWindow;
            _gracefulTerminationSeconds = gracefulTerminationSeconds;
        }

        public async Task RunAsync(string inputPath, string outputDirPath, OnTransmuxerStarted? onStarted, OnTransmuxerEnded? onEnded, CancellationToken cancellation)
        {
            new DirectoryInfo(outputDirPath).Create();
            var outputPath = Path.Combine(outputDirPath, _outputFileName);

            using var process = new Process();

            try
            {
                await RunProcessAsync(inputPath, outputPath, process, onStarted, cancellation);

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
                    await onEnded.Invoke(outputPath);
            }
        }

        private async Task RunProcessAsync(string inputPath, string outputPath, Process process, OnTransmuxerStarted? onStarted, CancellationToken cancellation)
        {
            var arguments = _arguments
                .Replace("{inputPath}", inputPath, StringComparison.InvariantCultureIgnoreCase)
                .Replace("{outputPath}", outputPath, StringComparison.InvariantCultureIgnoreCase);

            process.StartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = arguments,
                CreateNoWindow = !_createWindow,
                UseShellExecute = _createWindow
            };

            if (!process.Start())
                throw new TransmuxerException("Error starting FFmpeg process");

            if (onStarted != null)
                await onStarted.Invoke(outputPath);

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
                    throw new TransmuxerException("Error shutting down FFmpeg process", shutdownEx);
                }
            }

            if (!process.HasExited)
                process.Kill();
        }
    }
}
