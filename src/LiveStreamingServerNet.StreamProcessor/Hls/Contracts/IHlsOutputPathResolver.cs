namespace LiveStreamingServerNet.StreamProcessor.Hls.Contracts
{
    public interface IHlsOutputPathResolver
    {
        ValueTask<string> ResolveOutputPath(IServiceProvider services, Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments);
    }
}
