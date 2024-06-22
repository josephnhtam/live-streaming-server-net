using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls;
using LiveStreamingServerNet.StreamProcessor.Internal.FFmpeg;
using LiveStreamingServerNet.StreamProcessor.Internal.FFprobe;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8Parsing;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Logging;
using LiveStreamingServerNet.Utilities.Common;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.AdaptiveTranscoding
{
    internal partial class AdaptiveHlsTranscoder : IStreamProcessor
    {
        private readonly IHlsCleanupManager _cleanupManager;
        private readonly Configuration _config;
        private readonly ILogger _logger;

        public string Name { get; }
        public Guid ContextIdentifier { get; }

        public AdaptiveHlsTranscoder(
            IHlsCleanupManager cleanupManager,
            Configuration config,
            ILogger<AdaptiveHlsTranscoder> logger)
        {
            DirectoryUtility.CreateDirectoryIfNotExists(Path.GetDirectoryName(config.ManifestOutputPath));

            Name = config.Name;
            ContextIdentifier = config.ContextIdentifier;

            _cleanupManager = cleanupManager;
            _config = config;
            _logger = logger;
        }

        public async Task RunAsync(
            string inputPath,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            OnStreamProcessorStarted? onStarted,
            OnStreamProcessorEnded? onEnded,
            CancellationToken cancellation)
        {
            var streamInfo = await ObtainStreamInformation(inputPath, cancellation);
            await StartTranscoding(inputPath, streamPath, streamArguments, streamInfo, onStarted, onEnded, cancellation);
        }

        private async Task<JsonDocument> ObtainStreamInformation(string inputPath, CancellationToken cancellation)
        {
            var config = new FFprobeProcess.Configuration
            {
                FFprobePath = _config.FFprobePath,
                GracefulTerminationSeconds = _config.FFprobeGracefulShutdownTimeoutSeconds,
                Arguments = "-v quiet -print_format json -show_streams {inputPath}"
            };

            try
            {
                var ffprobeProcess = new FFprobeProcess(config);
                var output = await ffprobeProcess.ExecuteAsync(inputPath, cancellation);
                return JsonDocument.Parse(output);
            }
            catch (Exception ex)
            {
                _logger.ObtainingStreamInformationError(inputPath, ex);
                throw;
            }
        }

        private async Task StartTranscoding(
            string inputPath,
            string streamPath,
            IReadOnlyDictionary<string, string> streamArguments,
            JsonDocument streamInfo,
            OnStreamProcessorStarted? onStarted,
            OnStreamProcessorEnded? onEnded,
            CancellationToken cancellation)
        {
            try
            {
                await PreRunAsync();

                var arguments = BuildFFmpegArguments(streamInfo);

                var ffmpegConfig = new FFmpegProcess.Configuration
                {
                    ContextIdentifier = ContextIdentifier,
                    Name = Name,
                    Arguments = arguments,
                    FFmpegPath = _config.FFmpegPath,
                    OutputPath = _config.ManifestOutputPath,
                    GracefulTerminationSeconds = _config.FFmpegGracefulTerminationSeconds
                };

                var ffmpegProcess = new FFmpegProcess(ffmpegConfig);
                await ffmpegProcess.RunAsync(inputPath, streamPath, streamArguments, onStarted, onEnded, cancellation);
            }
            finally
            {
                await PostRunAsync();
            }
        }

        private async ValueTask PreRunAsync()
        {
            await ExecuteCleanupAsync();
        }

        private async ValueTask PostRunAsync()
        {
            await ScheduleCleanupAsync();
        }

        private async Task ExecuteCleanupAsync()
        {
            if (!_config.CleanupDelay.HasValue)
                return;

            await _cleanupManager.ExecuteCleanupAsync(_config.ManifestOutputPath);
        }

        private async ValueTask ScheduleCleanupAsync()
        {
            if (!_config.CleanupDelay.HasValue)
                return;

            try
            {
                var manifestOutputPath = _config.ManifestOutputPath;
                var dirPath = Path.GetDirectoryName(manifestOutputPath) ?? string.Empty;

                var playlist = ManifestParser.Parse(manifestOutputPath);
                var cleanupDelay = CalculateCleanupDelay(playlist.TsSegments.ToList(), _config.CleanupDelay.Value);

                var files = new List<string> { manifestOutputPath };
                files.AddRange(playlist.Manifests.Values.Select(x => Path.Combine(dirPath, x.Name)));
                files.AddRange(playlist.TsSegments.Select(x => Path.Combine(dirPath, x.FileName)));

                await _cleanupManager.ScheduleCleanupAsync(manifestOutputPath, files, cleanupDelay);
            }
            catch (Exception ex)
            {
                _logger.SchedulingHlsCleanupError(_config.ManifestOutputPath, ex);
            }
        }

        private static TimeSpan CalculateCleanupDelay(IList<ManifestTsSegment> tsSegments, TimeSpan cleanupDelay)
        {
            if (!tsSegments.Any())
                return TimeSpan.Zero;

            var (count, maxDuration) = tsSegments
                .GroupBy(x => x.ManifestName)
                .Select(x => (Count: x.Count(), MaxDuration: x.ToList().Max(x => x.Duration)))
                .MaxBy(x => x.MaxDuration);

            return TimeSpan.FromSeconds(count * maxDuration) + cleanupDelay;
        }
    }
}
