namespace LiveStreamingServerNet.Utilities.Contracts
{
    public interface IItemCollection<TItem>
    {
        TItem Get(string key);
    }
}
