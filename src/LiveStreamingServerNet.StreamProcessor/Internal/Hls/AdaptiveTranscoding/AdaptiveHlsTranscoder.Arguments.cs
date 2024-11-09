using LiveStreamingServerNet.StreamProcessor.Exceptions;
using LiveStreamingServerNet.StreamProcessor.Hls.Configurations;
using System.Text;
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

            AddComplexFilter(downsamplingFilters, arguments);

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
            IEnumerable<DownsamplingFilter> downsamplingFilters)
        {
            var videoHeight = GetVideoHeight(streamInfo);
            var result = downsamplingFilters.Where(x => x.Height <= videoHeight).ToList();
            return result.Any() ? result : downsamplingFilters.Take(1).ToList();
        }

        private void AddComplexFilter(IList<DownsamplingFilter> downsamplingFilters, List<string> arguments)
        {
            arguments.Add($"-filter_complex \"{BuildComplexFilter(downsamplingFilters)}\"");
        }

        private string BuildComplexFilter(IList<DownsamplingFilter> downsamplingFilters)
        {
            var sb = new StringBuilder();

            AppendAdditionalFilters(sb);

            var videoSrc = AppendPreSplitVideoFilters(sb);
            AppendVideoSplit(downsamplingFilters, sb, videoSrc);
            AppendVideoFilters(downsamplingFilters, sb);

            var audioSrc = AppendPreSplitAudioFilters(sb);
            AppendAudioSplit(downsamplingFilters, sb, audioSrc);
            AppendAudioFilters(downsamplingFilters, sb);

            return sb.ToString();

            void AppendAdditionalFilters(StringBuilder sb)
            {
                if (_config.AdditionalComplexFilters?.Any() != true)
                    return;

                foreach (var filter in _config.AdditionalComplexFilters)
                {
                    sb.Append(filter);
                    sb.Append(";");
                }
            }

            string AppendPreSplitVideoFilters(StringBuilder sb)
            {
                var src = "0:v";

                if (_config.VideoFilters?.Any() == true)
                {
                    string next = src;

                    var filters = _config.VideoFilters.ToList();
                    for (int i = 0; i < filters.Count; i++)
                    {
                        sb.Append($"[{next}]");
                        sb.Append(filters[i]);

                        next = $"vsrc-{i}";
                        sb.Append($"[{next}];");
                    }

                    return next;
                }

                return src;
            }

            string AppendPreSplitAudioFilters(StringBuilder sb)
            {
                var src = "0:a";

                if (_config.AudioFilters?.Any() == true)
                {
                    string next = src;

                    var filters = _config.AudioFilters.ToList();
                    for (int i = 0; i < filters.Count; i++)
                    {
                        sb.Append($"[{next}]");
                        sb.Append(filters[i]);

                        next = $"asrc-{i}";
                        sb.Append($"[{next}];");
                    }

                    return next;
                }

                return src;
            }

            static void AppendVideoSplit(IList<DownsamplingFilter> downsamplingFilters, StringBuilder sb, string src)
            {
                sb.Append($"[{src}]split=");
                sb.Append(downsamplingFilters.Count);

                for (int i = 0; i < downsamplingFilters.Count; i++)
                {
                    var filter = downsamplingFilters[i];
                    sb.Append($"[vout{i}-0]");
                }

                sb.Append(";");
            }

            static void AppendVideoFilters(IList<DownsamplingFilter> downsamplingFilters, StringBuilder sb)
            {
                for (int i = 0; i < downsamplingFilters.Count; i++)
                {
                    var filter = downsamplingFilters[i];

                    var videoFilters = new List<string> { $"scale=-2:{filter.Height}" };
                    videoFilters.AddRange(filter.VideoFilter ?? Enumerable.Empty<string>());

                    foreach (var (videoFilter, index) in videoFilters.Select((vf, idx) => (vf, idx)))
                    {
                        sb.Append($"[vout{i}-{index}]");
                        sb.Append(videoFilter);
                        sb.Append($"[vout{i}-{index + 1}];");
                    }
                }
            }

            static void AppendAudioSplit(IList<DownsamplingFilter> downsamplingFilters, StringBuilder sb, string src)
            {
                sb.Append($"[{src}]asplit=");
                sb.Append(downsamplingFilters.Count);

                for (int i = 0; i < downsamplingFilters.Count; i++)
                {
                    var filter = downsamplingFilters[i];
                    sb.Append($"[aout{i}-0]");
                }

                sb.Append(";");
            }

            static void AppendAudioFilters(IList<DownsamplingFilter> downsamplingFilters, StringBuilder sb)
            {
                for (int i = 0; i < downsamplingFilters.Count; i++)
                {
                    var audioFilters = downsamplingFilters[i].AudioFilter ?? Enumerable.Empty<string>();
                    foreach (var (audioFilter, index) in audioFilters.Select((af, idx) => (af, idx)))
                    {
                        sb.Append($"[aout{i}-{index}]");
                        sb.Append(audioFilter);
                        sb.Append($"[aout{i}-{index + 1}];");
                    }
                }
            }
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
                var lastIndex = 1 + (filter.VideoFilter?.Count() ?? 0);
                arguments.Add($"-map \"[vout{i}-{lastIndex}]\"");

                AddOptionalArgument(arguments, filter.VideoEncodingArgument?.Invoke(i));
                arguments.Add($"-maxrate:v:{i} {filter.MaxVideoBitrate}");
            }

            static void MapAudioStream(List<string> arguments, int i, DownsamplingFilter filter)
            {
                var lastIndex = filter.AudioFilter?.Count() ?? 0;
                arguments.Add($"-map \"[aout{i}-{lastIndex}]\"");

                AddOptionalArgument(arguments, filter.AudioEncodingArgument?.Invoke(i));
                arguments.Add($"-b:a:{i} {filter.MaxAudioBitrate}");
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
