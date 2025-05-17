using LiveStreamingServerNet.StreamProcessor.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Transcriptions.Contracts
{
    /// <summary>
    /// Provides a factory contract for creating instances of <see cref="ITranscriptionStream"/>.
    /// </summary>
    public interface ITranscriptionStreamFactory
    {
        /// <summary>
        /// Creates a new instance of a transcription stream.
        /// </summary>
        /// <param name="inputStreamWriterFactory">The media stream writer factory used for creating writers for the transcription stream.</param>
        /// <returns>An instance of <see cref="ITranscriptionStream"/>.</returns>
        ITranscriptionStream Create(IMediaStreamWriterFactory inputStreamWriterFactory);
    }
}
