namespace LiveStreamingServerNet.Transmuxer.Contracts
{
    public interface ITransmuxer
    {
        Task RunAsync(string inputPath, string outputPath, CancellationToken cancellation);
    }
}
