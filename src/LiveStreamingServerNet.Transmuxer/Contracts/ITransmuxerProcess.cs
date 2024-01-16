namespace LiveStreamingServerNet.Transmuxer.Contracts
{
    public interface ITransmuxerProcess
    {
        Task RunAsync(string inputPath, string outputPath, CancellationToken cancellation);
    }
}
