namespace LiveStreamingServerNet.Rtmp.Server.Internal.Filtering.Contracts
{
    internal interface IFilter<TItem> where TItem : struct
    {
        bool IsAllowed(TItem codec);
    }
}
