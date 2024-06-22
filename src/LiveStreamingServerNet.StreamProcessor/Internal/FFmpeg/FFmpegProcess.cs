using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Exceptions;
using LiveStreamingServerNet.Utilities.Common;
using System.Diagnostics;

namespace LiveStreamingServerNet.StreamProcessor.Internal.FFmpeg
{
    internal partial class FFmpegProcess : IStreamProcessor
    {
        private readonly Configuration _config;

        public string Name { get; }
        public Guid ContextIdentifier { get; }

        public FFmpegProcess(Configuration config)
        {
            Name = config.Name;
            ContextIdentifier = config.ContextIdentifier;
            _config = config;
        }

        public async Task RunAsync(
            string inputPath,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            OnStreamProcessorStarted? onStarted,
            OnStreamProcessorEnded? onEnded,
            CancellationToken cancellation)
        {
            DirectoryUtility.CreateDirectoryIfNotExists(Path.GetDirectoryName(_config.OutputPath));
            await RunProcessAsync(inputPath, _config.OutputPath, onStarted, onEnded, cancellation);
        }

        private async Task RunProcessAsync(string inputPath, string outputPath, OnStreamProcessorStarted? onStarted, OnStreamProcessorEnded? onEnded, CancellationToken cancellation)
        {
            var arguments = _config.Arguments
                .Replace("{inputPath}", inputPath, StringComparison.InvariantCultureIgnoreCase)
                .Replace("{outputPath}", outputPath, StringComparison.InvariantCultureIgnoreCase);

            using var process = new Process();

            try
            {
                if (!string.IsNullOrWhiteSpace(outputPath))
                    File.Delete(outputPath);

                process.StartInfo = new ProcessStartInfo
                {
                    FileName = _config.FFmpegPath,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                };

                if (!process.Start())
                    throw new StreamProcessorException("Error starting FFmpeg process");

                if (onStarted != null)
                    await onStarted.Invoke(outputPath);

                await process.WaitForExitAsync(cancellation);

                if (process.ExitCode != 0)
                    throw new StreamProcessorException($"FFmpeg process exited with code {process.ExitCode}");
            }
            catch (Exception ex)
            {
                await WaitForProcessTerminatingGracefully(process, _config.GracefulTerminationSeconds);

                if (ex is OperationCanceledException && cancellation.IsCancellationRequested)
                    throw;

                throw new StreamProcessorException("Error running FFmpeg process", ex);
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
                    throw new StreamProcessorException("Error shutting down FFmpeg process", shutdownEx);
                }
            }

            if (!process.HasExited)
                process.Kill();
        }
    }
}
