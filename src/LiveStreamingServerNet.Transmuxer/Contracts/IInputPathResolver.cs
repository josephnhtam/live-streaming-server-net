namespace LiveStreamingServerNet.Transmuxer.Contracts
{
    public interface IInputPathResolver
    {
        Task<string> ResolveInputPathAsync(string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
