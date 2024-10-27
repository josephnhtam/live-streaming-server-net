using LiveStreamingServerNet.StreamProcessor.Contracts;

namespace LiveStreamingServerNet.StreamProcessor
{
    /// <summary>
    /// Default implementation of stream processor condition.
    /// Always returns true.
    /// </summary>
    public class DefaultStreamProcessorCondition : IStreamProcessorCondition
    {
        public ValueTask<bool> IsEnabled(IServiceProvider services, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            return ValueTask.FromResult(true);
        }
    }
}
