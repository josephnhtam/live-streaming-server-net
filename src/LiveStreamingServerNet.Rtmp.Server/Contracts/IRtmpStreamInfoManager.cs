namespace LiveStreamingServerNet.Rtmp.Server.Contracts
{
    public interface IRtmpStreamInfoManager
    {
        IList<string> GetStreamPaths();
        IList<IRtmpStreamInfo> GetStreamInfos();
        IRtmpStreamInfo? GetStreamInfo(string streamPath);
    }
}
