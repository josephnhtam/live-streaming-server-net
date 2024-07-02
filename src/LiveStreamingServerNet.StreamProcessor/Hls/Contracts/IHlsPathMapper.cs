namespace LiveStreamingServerNet.StreamProcessor.Hls.Contracts
{
    public interface IHlsPathMapper
    {
        string? GetHlsOutputPath(string streamPath);
    }
}
