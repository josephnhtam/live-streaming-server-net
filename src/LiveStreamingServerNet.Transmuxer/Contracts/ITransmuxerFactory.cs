namespace LiveStreamingServerNet.Transmuxer.Contracts
{
    public interface ITransmuxerProcessFactory
    {
        Task<ITransmuxer> CreateAsync(string streamPath, IDictionary<string, string> streamArguments);
    }
}
