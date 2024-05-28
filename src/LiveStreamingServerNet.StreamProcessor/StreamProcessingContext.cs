namespace LiveStreamingServerNet.StreamProcessor
{
    public record StreamProcessingContext(
        string Processor,
        Guid Identifier,
        uint ClientId,
        string InputPath,
        string OutputPath,
        string StreamPath,
        IReadOnlyDictionary<string, string> StreamArguments);
}
