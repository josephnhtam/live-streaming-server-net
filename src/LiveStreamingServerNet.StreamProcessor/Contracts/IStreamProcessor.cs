namespace LiveStreamingServerNet.StreamProcessor.Contracts
{
    public delegate Task OnStreamProcessorStarted(string outputPath);
    public delegate Task OnStreamProcessorEnded(string outputPath);

    public interface IStreamProcessor
    {
        string Name { get; }
        Guid ContextIdentifier { get; }
        Task RunAsync(string inputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments, OnStreamProcessorStarted? onStarted, OnStreamProcessorEnded? onEnded, CancellationToken cancellation);
    }
}
