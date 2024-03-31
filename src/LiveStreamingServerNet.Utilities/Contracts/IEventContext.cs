namespace LiveStreamingServerNet.Utilities.Contracts
{
    public interface IEventContext
    {
        IDictionary<string, object?> Items { get; }
        TFeature? Get<TFeature>() where TFeature : class;
        void Set<TFeature>(TFeature instance) where TFeature : class;
    }
}
