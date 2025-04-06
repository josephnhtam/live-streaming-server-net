namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Output
{
    internal record struct SubtitleTranscriberConfiguration(
        Guid ContextIdentifier,
        string StreamPath,
        string TransmuxerName,
        string SubtitleManifestOutputPath,
        string SubtitleSegmentOutputPath
    );
}
