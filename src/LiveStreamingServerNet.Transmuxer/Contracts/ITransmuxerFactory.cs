namespace LiveStreamingServerNet.Transmuxer.Contracts
{
    public interface ITransmuxerFactory
    {
        Task<ITransmuxer> CreateAsync(Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
