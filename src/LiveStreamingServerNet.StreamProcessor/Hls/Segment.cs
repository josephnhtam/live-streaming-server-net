namespace LiveStreamingServerNet.StreamProcessor.Hls
{
    public record struct Segment(string ManifestName, string FileName, float Duration);
}
