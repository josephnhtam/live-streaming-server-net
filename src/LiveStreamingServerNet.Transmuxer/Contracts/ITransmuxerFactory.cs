namespace LiveStreamingServerNet.Transmuxer.Contracts
{
    public interface ITransmuxerFactory
    {
        Task<ITransmuxer> CreateAsync(string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
