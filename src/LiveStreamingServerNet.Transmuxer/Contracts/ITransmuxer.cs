namespace LiveStreamingServerNet.Transmuxer.Contracts
{
    public delegate Task OnTransmuxerStarted(string identifier, string outputPath);
    public delegate Task OnTransmuxerEnded(string identifier, string outputPath);

    public interface ITransmuxer
    {
        Task RunAsync(string inputPath, OnTransmuxerStarted? onStarted, OnTransmuxerEnded? onEnded, CancellationToken cancellation);
    }
}
