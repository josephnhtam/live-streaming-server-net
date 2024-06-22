namespace LiveStreamingServerNet.Utilities.Common.Contracts
{
    public interface IItemCollection<TItem>
    {
        TItem Get(string key);
    }
}
