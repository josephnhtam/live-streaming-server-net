using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Exceptions;
using System.Diagnostics;

namespace LiveStreamingServerNet.Transmuxer
{
    public class FFmpegTransmuxer : ITransmuxer
    {
        private readonly string _ffmpegPath;
        private readonly string _arguments;
        private readonly int _gracefulTerminationSeconds;
        private readonly string _outputPath;

        public string Name { get; }
        public Guid ContextIdentifier { get; }

        public FFmpegTransmuxer(Guid contextIdentifier, string name, string ffmpegPath, string arguments, int gracefulTerminationSeconds, string outputPath)
        {
            ContextIdentifier = contextIdentifier;
            Name = name;
            _ffmpegPath = ffmpegPath;
            _arguments = arguments;
            _gracefulTerminationSeconds = gracefulTerminationSeconds;
            _outputPath = outputPath;
        }

        public async Task RunAsync(
            string inputPath,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            OnTransmuxerStarted? onStarted,
            OnTransmuxerEnded? onEnded,
            CancellationToken cancellation)
        {
            new DirectoryInfo(Path.GetDirectoryName(_outputPath)!).Create();
            await RunProcessAsync(inputPath, _outputPath, onStarted, onEnded, cancellation);
        }

        private async Task RunProcessAsync(string inputPath, string outputPath, OnTransmuxerStarted? onStarted, OnTransmuxerEnded? onEnded, CancellationToken cancellation)
        {
            var arguments = _arguments
                .Replace("{inputPath}", inputPath, StringComparison.InvariantCultureIgnoreCase)
                .Replace("{outputPath}", outputPath, StringComparison.InvariantCultureIgnoreCase);

            using var process = new Process();

            try
            {
                File.Delete(outputPath);

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
                    await onStarted.Invoke(outputPath);

                await process.WaitForExitAsync(cancellation);

                if (process.ExitCode != 0)
                    throw new TransmuxerException($"FFmpeg process exited with code {process.ExitCode}");
            }
            catch (Exception ex)
            {
                await WaitForProcessTerminatingGracefully(process, _gracefulTerminationSeconds);

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

        private static async Task WaitForProcessTerminatingGracefully(Process process, int gracefulPeriod)
        {
            if (!process.HasExited)
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
