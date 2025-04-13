namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders.Contracts
{
    internal interface IMediaManifestBuilder : IManifestBuilder
    {
        IMediaManifestBuilder SetTargetDuration(TimeSpan targetDuration);
        IMediaManifestBuilder SetMediaSequence(uint sequenceNumber);
        IMediaManifestBuilder SetAllowCache(bool allowCache);
        IMediaManifestBuilder SetIndependentSegments(bool includeIndependentSegments);
        IMediaManifestBuilder SetInitialProgramDateTime(DateTime programDateTime);
        IMediaManifestBuilder AddSegment(MediaSegment segment);
    }
}
