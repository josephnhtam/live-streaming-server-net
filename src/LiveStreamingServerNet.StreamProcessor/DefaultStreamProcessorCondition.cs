using LiveStreamingServerNet.StreamProcessor.Contracts;

namespace LiveStreamingServerNet.StreamProcessor
{
    internal class DefaultStreamProcessorCondition : IStreamProcessorCondition
    {
        public ValueTask<bool> IsEnabled(IServiceProvider services, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            return ValueTask.FromResult(true);
        }
    }
}
