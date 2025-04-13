using LiveStreamingServerNet.StreamProcessor.Hls.Subtitling.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling
{
    internal class SubtitleCueExtractorFactory : ISubtitleCueExtractorFactory
    {
        public ISubtitleCueExtractor Create()
        {
            return new SubtitleCueExtractor();
        }
    }
}
