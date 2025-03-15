namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders.Contracts
{
    internal interface IVideoManifestBuilder : IMediaManifestBuilder
    {
        IVideoManifestBuilder AddVideoSegment(string uri, TimeSpan duration, string? title = null);
    }
}
