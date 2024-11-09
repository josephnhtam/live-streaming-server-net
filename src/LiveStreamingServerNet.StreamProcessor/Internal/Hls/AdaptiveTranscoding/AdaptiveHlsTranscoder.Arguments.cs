using LiveStreamingServerNet.StreamProcessor.Exceptions;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using System.Text.Json;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.AdaptiveTranscoding
{
    internal partial class AdaptiveHlsTranscoder
    {
        private string BuildFFmpegArguments(JsonDocument streamInfo)
        {
            GetManifestPath(_config.ManifestOutputPath, out var masterManifestName, out var manifestPath);
            var downsamplingFilters = GetDownsamplingFilters(streamInfo, _config.DownsamplingFilters);

            var arguments = new List<string>();

            AddDecodingArguments(arguments);

            AddInputs(arguments);

            AddPerformanceOptions(arguments);

            AddEncodingArguments(arguments);

            AddStreamMappings(downsamplingFilters, arguments);

            AddHlsConfiguration(downsamplingFilters, arguments);

            AddMasterManifest(masterManifestName, manifestPath, arguments);

            return string.Join(' ', arguments);
        }

        private static void AddMasterManifest(string masterManifestName, string manifestPath, List<string> arguments)
        {
            arguments.Add($"-master_pl_name {masterManifestName} {manifestPath}");
        }

        private void AddHlsConfiguration(IList<DownsamplingFilter> downsamplingFilters, List<string> arguments)
        {
            arguments.Add("-f hls");
            arguments.Add($"-hls_list_size {_config.HlsOptions.SegmentListSize}");
            arguments.Add($"-hls_time {_config.HlsOptions.SegmentLength.TotalSeconds}");

            var hlsFlags = CreateHlsFlags(_config.HlsOptions);
            if (hlsFlags.Any()) arguments.Add($"-hls_flags {string.Join('+', hlsFlags)}");

            AddOptionalArgument(arguments, _config.HlsOptions.ExtraArguments);

            arguments.Add($"-var_stream_map \"{string.Join(' ', CreateStreamMap(downsamplingFilters))}\"");
        }

        private void AddEncodingArguments(List<string> arguments)
        {
            AddOptionalArgument(arguments, _config.VideoEncodingArguments?.Trim());
            AddOptionalArgument(arguments, _config.AudioEncodingArguments?.Trim());
        }

        private void AddInputs(List<string> arguments)
        {
            arguments.Add("-i {inputPath}");

            if (_config.AdditionalInputs?.Any() == true)
            {
                foreach (var input in _config.AdditionalInputs)
                {
                    arguments.Add($"-i {input}");
                }
            }
        }

        private void AddDecodingArguments(List<string> arguments)
        {
            AddOptionalArgument(arguments, _config.VideoDecodingArguments);
            AddOptionalArgument(arguments, _config.AudioDecodingArguments);
        }

        private void AddPerformanceOptions(List<string> arguments)
        {
            arguments.Add($"-threads {_config.PerformanceOptions.Threads}");

            if (_config.PerformanceOptions.MaxMuxingQueueSize.HasValue)
                arguments.Add($"-max_muxing_queue_size {_config.PerformanceOptions.MaxMuxingQueueSize.Value}");

            AddOptionalArgument(arguments, _config.PerformanceOptions.ExtraArguments);
        }

        private static void AddOptionalArgument(IList<string> arguments, string? optionalArgument)
        {
            if (string.IsNullOrWhiteSpace(optionalArgument))
                return;

            arguments.Add(optionalArgument.Trim());
        }

        private static int GetVideoHeight(JsonDocument streamInfo)
        {
            if (streamInfo.RootElement.TryGetProperty("streams", out var streams))
            {
                foreach (var stream in streams.EnumerateArray())
                {
                    if (!stream.TryGetProperty("codec_type", out var codecType) ||
                        codecType.GetString() != "video")
                        continue;

                    if (stream.TryGetProperty("height", out var height) && height.TryGetInt32(out var heightValue))
                        return heightValue;
                }
            }

            throw new StreamProbeException("Failed to obtain video height from stream information.");
        }

        private static IList<DownsamplingFilter> GetDownsamplingFilters(
            JsonDocument streamInfo,
            IList<DownsamplingFilter> downsamplingFilters)
        {
            var videoHeight = GetVideoHeight(streamInfo);
            var result = downsamplingFilters.Where(x => x.Height <= videoHeight).ToList();
            return result.Any() ? result : downsamplingFilters.Take(1).ToList();
        }

        private static void AddStreamMappings(IList<DownsamplingFilter> downsamplingFilters, List<string> arguments)
        {
            for (int i = 0; i < downsamplingFilters.Count; i++)
            {
                var filter = downsamplingFilters[i];
                MapVideoStream(arguments, i, filter);
                MapAudioStream(arguments, i, filter);
            }

            static void MapVideoStream(List<string> arguments, int i, DownsamplingFilter filter)
            {
                arguments.Add($"-map 0:v:0");

                AddOptionalArgument(arguments, filter.VideoEncodingArgument?.Invoke(i));
                arguments.Add($"-maxrate:v:{i} {filter.MaxVideoBitrate}");

                var videoFilter = CreateVideoFilter(filter);
                arguments.Add($"-filter:v:{i} \"{videoFilter}\"");
            }

            static void MapAudioStream(List<string> arguments, int i, DownsamplingFilter filter)
            {
                arguments.Add("-map 0:a:0");

                AddOptionalArgument(arguments, filter.AudioEncodingArgument?.Invoke(i));
                arguments.Add($"-b:a:{i} {filter.MaxAudioBitrate}");

                var audioFilter = CreateAudioFilter(filter);
                if (audioFilter != null) arguments.Add($"-filter:a:{i} \"{audioFilter}\"");
            }

            static string CreateVideoFilter(DownsamplingFilter filter)
            {
                var videoFilters = new List<string> { $"scale=-2:{filter.Height}" };

                if (filter.VideoFilter != null)
                    videoFilters.AddRange(filter.VideoFilter);

                return string.Join(",", videoFilters);
            }

            static string? CreateAudioFilter(DownsamplingFilter filter)
            {
                if (filter.AudioFilter != null)
                    return string.Join(",", filter.AudioFilter);

                return null;
            }
        }

        private static IList<string> CreateStreamMap(IList<DownsamplingFilter> downsamplingFilters)
        {
            var streamMap = new List<string>();

            for (int i = 0; i < downsamplingFilters.Count; i++)
            {
                var filter = downsamplingFilters[i];
                streamMap.Add($"v:{i},a:{i},name:{filter.Name}");
            }

            return streamMap;
        }

        private static IList<string> CreateHlsFlags(HlsOptions hlsSettings)
        {
            var hlsFlags = new List<string>() { "independent_segments" };

            if (hlsSettings.DeleteOutdatedSegments)
                hlsFlags.Add("delete_segments");

            if (hlsSettings.Flags != null)
                hlsFlags.AddRange(hlsSettings.Flags);

            return hlsFlags;
        }

        private static void GetManifestPath(string outputPath, out string masterManifestName, out string manifestPath)
        {
            var dir = Path.GetDirectoryName(outputPath) ?? string.Empty;
            var outputName = Path.GetFileNameWithoutExtension(outputPath);
            var outputExtension = Path.GetExtension(outputPath);

            masterManifestName = outputName + outputExtension;
            manifestPath = Path.Combine(dir, $"{outputName}_%v{outputExtension}");
        }
    }
}
