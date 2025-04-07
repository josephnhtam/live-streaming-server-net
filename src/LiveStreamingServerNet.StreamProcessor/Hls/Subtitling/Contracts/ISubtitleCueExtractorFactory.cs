namespace LiveStreamingServerNet.StreamProcessor.Hls.Subtitling.Contracts
{
    public interface ISubtitleCueExtractorFactory
    {
        ISubtitleCueExtractor Create();
    }
}
