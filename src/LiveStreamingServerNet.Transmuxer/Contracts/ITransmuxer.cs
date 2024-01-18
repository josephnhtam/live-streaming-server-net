namespace LiveStreamingServerNet.Transmuxer.Contracts
{
    public delegate Task OnTransmuxerStarted(string outputPath);
    public delegate Task OnTransmuxerEnded(string outputPath);

    public interface ITransmuxer
    {
        Task RunAsync(string inputPath, string outputDirPath, OnTransmuxerStarted? onStarted, OnTransmuxerEnded? onEnded, CancellationToken cancellation);
    }
}
