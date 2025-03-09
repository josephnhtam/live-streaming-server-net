namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders.Contracts
{
    internal interface ISubtitleManifestBuilder : IMediaManifestBuilder
    {
        ISubtitleManifestBuilder AddSubtitleSegment(string uri, TimeSpan duration, string? title = null);
    }
}
