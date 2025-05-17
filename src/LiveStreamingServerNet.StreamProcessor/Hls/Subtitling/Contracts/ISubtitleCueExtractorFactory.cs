namespace LiveStreamingServerNet.StreamProcessor.Hls.Subtitling.Contracts
{
    /// <summary>
    /// Defines a factory interface responsible for creating instances of <see cref="ISubtitleCueExtractor"/>.
    /// </summary>
    public interface ISubtitleCueExtractorFactory
    {
        /// <summary>
        /// Creates a new instance of a subtitle cue extractor.
        /// </summary>
        ISubtitleCueExtractor Create();
    }
}
