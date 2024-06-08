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

            AddOptionalArgument(arguments, _config.VideoDecodingArguments);
            AddOptionalArgument(arguments, _config.AudioDecodingArguments);

            arguments.Add("-i {inputPath}");

            arguments.Add($"-threads {_config.PerformanceOptions.Threads}");
            AddOptionalArgument(arguments, _config.PerformanceOptions.ExtraArguments);

            arguments.Add($"{_config.VideoEncodingArguments.Trim()}");
            arguments.Add($"{_config.AudioEncodingArguments.Trim()}");

            AddDownsamplingFilters(downsamplingFilters, arguments);

            arguments.Add("-f hls");
            arguments.Add($"-hls_list_size {_config.HlsOptions.SegmentListSize}");
            arguments.Add($"-hls_time {_config.HlsOptions.SegmentLength.TotalSeconds}");

            var hlsFlags = CreateHlsFlags(_config.HlsOptions);
            if (hlsFlags.Any()) arguments.Add($"-hls_flags {string.Join('+', hlsFlags)}");

            AddOptionalArgument(arguments, _config.HlsOptions.ExtraArguments);

            arguments.Add($"-var_stream_map \"{string.Join(' ', CreateStreamMap(downsamplingFilters))}\"");
            arguments.Add($"-master_pl_name {masterManifestName} {manifestPath}");

            return string.Join(' ', arguments);
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

        private static void AddDownsamplingFilters(IList<DownsamplingFilter> downsamplingFilters, List<string> arguments)
        {
            for (int i = 0; i < downsamplingFilters.Count; i++)
            {
                var filter = downsamplingFilters[i];

                arguments.Add("-map 0:v:0");
                arguments.Add("-map 0:a:0");

                arguments.Add($"-filter:v:{i} scale=-2:{filter.Height}");
                arguments.Add($"-maxrate:v:{i} {filter.MaxVideoBitrate}");
                arguments.Add($"-b:a:{i} {filter.MaxAudioBitrate}");

                AddOptionalArgument(arguments, filter.ExtraArguments);
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
