namespace LiveStreamingServerNet.Rtmp.Contracts
{
    public interface IRtmpStreamInfoManager
    {
        IList<string> GetStreamPaths();
        IList<IRtmpStreamInfo> GetStreamInfos();
        IRtmpStreamInfo? GetStreamInfo(string streamPath);
    }
}
