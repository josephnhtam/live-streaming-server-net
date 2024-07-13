using LiveStreamingServerNet.AdminPanelUI.Dtos;
using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using Riok.Mapperly.Abstractions;

namespace LiveStreamingServerNet.Standalone.Internal.Mappers
{
    [Mapper]
    internal static partial class StreamMapper
    {
        public static StreamDto ToDto(this IRtmpStream stream)
        {
            var dto = ConvertToDto(stream);

            if (stream.MetaData != null)
            {
                dto.VideoCodecId = stream.MetaData.GetIntValue("videocodecid", 0);
                dto.Height = stream.MetaData.GetIntValue("height", 0);
                dto.Width = stream.MetaData.GetIntValue("width", 0);
                dto.Framerate = stream.MetaData.GetIntValue("framerate", 0);

                dto.AudioCodecId = stream.MetaData.GetIntValue("audiocodecid", 0);
                dto.AudioSampleRate = stream.MetaData.GetIntValue("audiosamplerate", 0);
                dto.AudioChannels = stream.MetaData.GetIntValue("audiochannels", 0);

                if (dto.AudioChannels == 0 && stream.MetaData.TryGetValue("stereo", out var _stereoValue) && _stereoValue is bool stereoValue)
                    dto.AudioChannels = stereoValue ? 2 : 1;
            }

            return dto;
        }

        private static int GetIntValue(this IReadOnlyDictionary<string, object> dictionary, string key, int defaultValue)
        {
            if (dictionary.TryGetValue(key, out var _doublValue) && _doublValue is double doubleValue)
                return (int)doubleValue;

            if (dictionary.TryGetValue(key, out var _intValue) && _intValue is int intValue)
                return intValue;

            return defaultValue;
        }

        [MapPropertyFromSource(nameof(StreamDto.Id), Use = nameof(MapId))]
        [MapProperty($"{nameof(IRtmpStream.Publisher)}.{nameof(IClientControl.ClientId)}", nameof(StreamDto.ClientId))]
        public static partial StreamDto ConvertToDto(IRtmpStream stream);

        private static string MapId(IRtmpStream stream)
            => $"{stream.Publisher.ClientId}@{stream.StreamPath}";
    }
}
