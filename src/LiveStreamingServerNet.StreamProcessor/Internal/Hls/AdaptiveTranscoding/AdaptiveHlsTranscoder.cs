using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Hls;
using LiveStreamingServerNet.StreamProcessor.Internal.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.FFmpeg;
using LiveStreamingServerNet.StreamProcessor.Internal.FFprobe;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Parsers;
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
        private readonly IHlsPathRegistry _pathRegistry;
        private readonly Configuration _config;
        private readonly ILogger _logger;

        private bool _registeredHlsOutputPath;

        public string Name { get; }
        public Guid ContextIdentifier { get; }
        public string StreamPath { get; }

        public AdaptiveHlsTranscoder(
            string streamPath,
            IHlsCleanupManager cleanupManager,
            IHlsPathRegistry pathRegistry,
            Configuration config,
            ILogger<AdaptiveHlsTranscoder> logger)
        {
            DirectoryUtility.CreateDirectoryIfNotExists(Path.GetDirectoryName(config.ManifestOutputPath));

            Name = config.Name;
            ContextIdentifier = config.ContextIdentifier;
            StreamPath = streamPath;

            _cleanupManager = cleanupManager;
            _pathRegistry = pathRegistry;
            _config = config;
            _logger = logger;
        }

        public async Task RunAsync(
            string inputPath,
            IReadOnlyDictionary<string, string> streamArguments,
            OnStreamProcessorStarted? onStarted,
            OnStreamProcessorEnded? onEnded,
            CancellationToken cancellation)
        {
            var streamInfo = await ObtainStreamInformation(inputPath, cancellation).ConfigureAwait(false);
            await StartTranscoding(inputPath, streamArguments, streamInfo, onStarted, onEnded, cancellation).ConfigureAwait(false);
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
                var output = await ffprobeProcess.ExecuteAsync(inputPath, cancellation).ConfigureAwait(false);
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
            IReadOnlyDictionary<string, string> streamArguments,
            JsonDocument streamInfo,
            OnStreamProcessorStarted? onStarted,
            OnStreamProcessorEnded? onEnded,
            CancellationToken cancellation)
        {
            try
            {
                await PreRunAsync().ConfigureAwait(false);

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

                var ffmpegProcess = new FFmpegProcess(StreamPath, ffmpegConfig, _logger);
                await ffmpegProcess.RunAsync(inputPath, streamArguments, onStarted, onEnded, cancellation).ConfigureAwait(false);
            }
            finally
            {
                await PostRunAsync().ConfigureAwait(false);
            }
        }

        private async ValueTask PreRunAsync()
        {
            RegisterHlsOutputPath();
            await ExecuteCleanupAsync().ConfigureAwait(false);
        }

        private async ValueTask PostRunAsync()
        {
            UnregisterHlsOutputPath();
            await ScheduleCleanupAsync().ConfigureAwait(false);
        }

        private void RegisterHlsOutputPath()
        {
            var outputPath = Path.GetDirectoryName(_config.ManifestOutputPath) ?? string.Empty;

            if (!_pathRegistry.RegisterHlsOutputPath(StreamPath, outputPath))
                throw new InvalidOperationException("A HLS output path of the same stream path is already registered");

            _registeredHlsOutputPath = true;
        }

        private void UnregisterHlsOutputPath()
        {
            if (!_registeredHlsOutputPath)
                return;

            _pathRegistry.UnregisterHlsOutputPath(StreamPath);
            _registeredHlsOutputPath = false;
        }

        private async Task ExecuteCleanupAsync()
        {
            if (!_config.CleanupDelay.HasValue)
                return;

            await _cleanupManager.ExecuteCleanupAsync(_config.ManifestOutputPath).ConfigureAwait(false);
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
                var cleanupDelay = CalculateCleanupDelay(playlist.Segments.ToList(), _config.CleanupDelay.Value);

                var files = new List<string> { manifestOutputPath };
                files.AddRange(playlist.Manifests.Values.Select(x => Path.Combine(dirPath, x.Name)));
                files.AddRange(playlist.Segments.Select(x => Path.Combine(dirPath, x.FileName)));

                await _cleanupManager.ScheduleCleanupAsync(manifestOutputPath, files, cleanupDelay).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.SchedulingHlsCleanupError(_config.ManifestOutputPath, ex);
            }
        }

        private static TimeSpan CalculateCleanupDelay(IList<Segment> segments, TimeSpan cleanupDelay)
        {
            if (!segments.Any())
                return TimeSpan.Zero;

            var (count, maxDuration) = segments
                .GroupBy(x => x.ManifestName)
                .Select(x => (Count: x.Count(), MaxDuration: x.ToList().Max(x => x.Duration)))
                .MaxBy(x => x.MaxDuration);

            return TimeSpan.FromSeconds(count * maxDuration) + cleanupDelay;
        }
    }
}
