namespace LiveStreamingServerNet.StreamProcessor.Contracts
{
    /// <summary>
    /// Provides a factory contract for creating instances of <see cref="ITranscodingStream"/>.
    /// </summary>
    public interface ITranscodingStreamFactory
    {
        /// <summary>
        /// Creates a new instance of a transcoding stream.
        /// </summary>
        ITranscodingStream Create();
    }
}