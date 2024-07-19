namespace LiveStreamingServerNet.Rtmp.Contracts
{
    public interface IRtmpStreamInfoManager
    {
        IList<string> GetStreamPaths();
        IList<IRtmpStreamInfo> GetStreamInfos();
        IRtmpStreamInfo? GetStreamInfo(string streamPath);
    }

    [Obsolete("Use IRtmpStreamInfoManager instead.")]
    public interface IRtmpStreamManager
    {
        IList<string> GetStreamPaths();
        IList<IRtmpStream> GetStreams();
        IRtmpStream? GetStream(string streamPath);
    }
}
