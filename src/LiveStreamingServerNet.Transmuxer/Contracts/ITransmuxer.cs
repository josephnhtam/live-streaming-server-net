namespace LiveStreamingServerNet.Transmuxer.Contracts
{
    public interface ITransmuxer
    {
        Task RunAsync(string inputPath, string outputDirPath, CancellationToken cancellation);
    }
}
