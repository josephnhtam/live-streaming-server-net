namespace LiveStreamingServerNet.Transmuxer
{
    public record TransmuxingContext(
        string Transmuxer,
        Guid Identifier,
        uint ClientId,
        string InputPath,
        string OutputPath,
        string StreamPath,
        IReadOnlyDictionary<string, string> StreamArguments);
}
