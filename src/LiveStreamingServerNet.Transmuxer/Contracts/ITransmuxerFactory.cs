namespace LiveStreamingServerNet.Transmuxer.Contracts
{
    public interface ITransmuxerFactory
    {
        Task<ITransmuxer> CreateAsync(string streamPath, IDictionary<string, string> streamArguments);
    }
}
