using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Standalone.Internal.Contracts;
using Riok.Mapperly.Abstractions;

namespace LiveStreamingServerNet.Standalone.Dtos.Mappers
{
    [Mapper]
    internal static partial class StreamMapper
    {
        public static StreamDto ToDto(this IRtmpPublishStream stream)
        {
            var dto = ConvertToDto(stream);

            if (stream.MetaData != null)
            {
                dto.VideoCodecId = (int)stream.MetaData.GetValueOrDefault("videocodecid", 0);
                dto.Height = (int)stream.MetaData.GetValueOrDefault("height", 0);
                dto.Weight = (int)stream.MetaData.GetValueOrDefault("width", 0);
                dto.Framerate = (int)stream.MetaData.GetValueOrDefault("framerate", 0);

                dto.AudioCodecId = (int)stream.MetaData.GetValueOrDefault("audiocodecid", 0);
                dto.AudioSampleRate = (int)stream.MetaData.GetValueOrDefault("audiosamplerate", 0);
                dto.AudioChannels = (int)stream.MetaData.GetValueOrDefault("audiochannels", 0);

                if (dto.AudioChannels == 0 && stream.MetaData.TryGetValue("stereo", out var _stereo) && _stereo is bool stereo)
                    dto.AudioChannels = stereo ? 2 : 1;
            }

            return dto;
        }

        [MapProperty([nameof(IRtmpPublishStream.Client), nameof(IClientControl.ClientId)], [nameof(StreamDto.ClientId)])]
        public static partial StreamDto ConvertToDto(IRtmpPublishStream stream);
    }
}
