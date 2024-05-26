namespace LiveStreamingServerNet.Rtmp.Contracts
{
    public interface IFilterBuilder<TItem> where TItem : struct
    {
        IFilterBuilder<TItem> Include(TItem codec);
        IFilterBuilder<TItem> Exclude(TItem codec);
    }
}