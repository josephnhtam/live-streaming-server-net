namespace LiveStreamingServerNet.Utilities.Contracts
{
    public interface IRentable<T> : IDisposable
    {
        T Value { get; }
    }
}
