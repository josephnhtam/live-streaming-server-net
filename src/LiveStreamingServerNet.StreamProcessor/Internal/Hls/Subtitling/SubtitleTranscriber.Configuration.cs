namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Subtitling
{
    internal record struct SubtitleTranscriberConfiguration(
        Guid ContextIdentifier,
        string StreamPath,
        string TransmuxerName,
        string SubtitleManifestOutputPath,
        string SubtitleSegmentOutputPath,
        bool DeleteOutdatedSegments,
        TimeSpan MinSegmentInterval
    );
}
