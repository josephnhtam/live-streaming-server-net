using LiveStreamingServerNet.StreamProcessor.Exceptions;
using System.Diagnostics;

namespace LiveStreamingServerNet.StreamProcessor.Internal.FFprobe
{
    internal partial class FFprobeProcess
    {
        private readonly Configuration _config;

        public FFprobeProcess(Configuration config)
        {
            _config = config;
        }

        public async Task<string> ExecuteAsync(string inputPath, CancellationToken cancellation)
        {
            var arguments = _config.Arguments
                .Replace("{inputPath}", inputPath, StringComparison.InvariantCultureIgnoreCase);

            using var process = new Process();

            try
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = _config.FFprobePath,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                if (!process.Start())
                    throw new StreamProbeException("Error starting FFprobe process");

                using var streamReader = new StreamReader(process.StandardOutput.BaseStream);
                var output = await streamReader.ReadToEndAsync(cancellation);

                await process.WaitForExitAsync(cancellation);

                if (process.ExitCode != 0)
                    throw new StreamProbeException($"FFprobe process exited with code {process.ExitCode}");

                return output;
            }
            catch (Exception ex)
            {
                await WaitForProcessTerminatingGracefully(process, _config.GracefulTerminationSeconds);

                if (ex is OperationCanceledException && cancellation.IsCancellationRequested)
                    throw;

                throw new StreamProcessorException("Error running FFprobe process", ex);
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
                catch { }
            }

            if (!process.HasExited)
                process.Kill();
        }
    }
}
