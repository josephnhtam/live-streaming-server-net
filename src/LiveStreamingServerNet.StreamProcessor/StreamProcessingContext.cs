namespace LiveStreamingServerNet.StreamProcessor
{
    /// <summary>
    /// Represents the context for stream processing operations.
    /// </summary>
    /// <param name="Processor">The identifier of the processor handling the stream.</param>
    /// <param name="Identifier">The unique identifier for this processing context.</param>
    /// <param name="ClientId">The identifier of the client requesting the stream processing.</param>
    /// <param name="InputPath">The path to the input stream source.</param>
    /// <param name="OutputPath">The path where processed stream will be output.</param>
    /// <param name="StreamPath">The path identifier for the stream being processed.</param>
    /// <param name="StreamArguments">Additional arguments associated with the stream processing.</param>
    public record StreamProcessingContext(
        string Processor,
        Guid Identifier,
        uint ClientId,
        string InputPath,
        string OutputPath,
        string StreamPath,
        IReadOnlyDictionary<string, string> StreamArguments);
}
