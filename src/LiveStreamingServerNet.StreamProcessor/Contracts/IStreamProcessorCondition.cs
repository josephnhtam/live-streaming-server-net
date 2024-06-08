namespace LiveStreamingServerNet.StreamProcessor.Contracts
{
    public interface IStreamProcessorCondition
    {
        ValueTask<bool> IsEnabled(IServiceProvider services, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
