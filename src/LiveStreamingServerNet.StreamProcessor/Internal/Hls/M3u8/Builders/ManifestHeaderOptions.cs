namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.M3u8.Builders
{
    internal class ManifestHeaderOptions
    {
        public string Version { get; set; } = "3";
        public bool AllowCache { get; set; } = false;
        public bool IncludeIndependentSegments { get; set; } = true;
    }
}
