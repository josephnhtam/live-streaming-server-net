namespace LiveStreamingServerNet.Transmuxer.Contracts
{
    public interface ITransmuxerProcessFactory
    {
        Task<ITransmuxerProcess> CreateAsync(string streamPath, IDictionary<string, string> streamArguments);
    }
}
