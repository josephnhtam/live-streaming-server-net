namespace LiveStreamingServerNet.Transmuxer.Contracts
{
    public interface IOutputPathResolver
    {
        Task<string> ResolveOutputPathAsync(string streamPath, IDictionary<string, string> streamArguments);
    }
}
