namespace LiveStreamingServerNet.Transmuxer.Contracts
{
    public interface IOutputDirectoryPathResolver
    {
        Task<string> ResolveOutputDirectoryPathAsync(string streamPath, IDictionary<string, string> streamArguments);
    }
}
