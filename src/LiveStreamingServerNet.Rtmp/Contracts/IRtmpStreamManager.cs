namespace LiveStreamingServerNet.Rtmp.Contracts
{
    public interface IRtmpStreamManager
    {
        IList<string> GetStreamPaths();
        IList<IRtmpStream> GetStreams();
        IRtmpStream? GetStream(string streamPath);
    }
}
