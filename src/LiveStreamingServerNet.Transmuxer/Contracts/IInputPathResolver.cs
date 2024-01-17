namespace LiveStreamingServerNet.Transmuxer.Contracts
{
    public interface IInputPathResolver
    {
        Task<string> ResolveInputPathAsync(string streamPath, IDictionary<string, string> streamArguments);
    }
}
